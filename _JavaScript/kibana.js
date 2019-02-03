Helper = {
    isNonEmptyString: function(x) {
        return (typeof x === 'string' || x instanceof String) && x.length !== 0;
    },
    sendPost: function(url, data) {
        return $.ajax(url, { "contentType": "application/json;charset=UTF-8", "method": "POST", "data":data });
    },
    delay: function(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    },
    reprocessAsync: async function(actionAsync, reprocessCount, delay) {
        for (let i = reprocessCount; i > 0; --i) {
            try {
                return await actionAsync();
            } catch {
                console.log("failed");
                await Helper.delay(delay);
            }
        }

        return await actionAsync();
    },
    load: function() {
        let loader = new KibanaLoader('Deployment:"master" && Code:"IncomingCall" && Message:"total response time"', ["Message"]);
        return loader.load(new TimeRange(new Date("2019-01-31T22:00:00"), new Date("2019-02-02T00:00:00")));
    }
};

function TimeRange(from, to) {
    if (from instanceof Date && to instanceof Date) {
        from = this.__getUtcEpochMills(from);
        to = this.__getUtcEpochMills(to);
    } else if (typeof from !== 'number' || typeof to !== 'number') {
        throw "from and to must be Dates or numbers.";
    }

    if (from > to)
        throw "from must be >= to.";

    this.from = from;
    this.to = to;
}
TimeRange.prototype.__getUtcEpochMills = function(date) {
    return date.getTime() - date.getTimezoneOffset() * 60000;
};
TimeRange.prototype.getIntersection = function(timeRange) {
    if (this.to < timeRange.from || timeRange.to < this.from)
        throw "time ranges don't intersect.";

    return new TimeRange(Math.max(this.from, timeRange.from), Math.min(this.to, timeRange.to));
};
TimeRange.prototype.getFirstHalf = function() {
    let diff = this.to - this.from;
    if (diff <= 0)
        throw "point doesn't have the half.";

    let newTo = this.to - Math.ceil(diff/2);
    return new TimeRange(this.from, newTo);
};
TimeRange.prototype.getSecondHalf = function() {
    let firstHalf = this.getFirstHalf();
    let newFrom = firstHalf.to + 1;
    return new TimeRange(newFrom, this.to);
};

function Index(name, timeRange) {
    if (!Helper.isNonEmptyString(name))
        throw "name must be non empty string";

    if (!(timeRange instanceof TimeRange))
        throw "timeRange must be TimeRange";

    this.name = name;
    this.timeRange = timeRange;
}

function KibanaLoader(queryString, fields) {
    if (!Helper.isNonEmptyString(queryString))
        throw "queryString must be non empty string.";

    if (!(fields instanceof Array) || fields.length === 0 || fields.some(x => !Helper.isNonEmptyString(x)))
        throw "fields should be non empty array of non empty strings.";

    this.queryString = queryString;
    this.fields = fields;
    this.requestSize = 10000;
}
KibanaLoader.prototype.load = async function(timeRange) {
    let indexes = await this.__requestToElsIndexes(timeRange);
    console.log(indexes);
    for (let i = 0; i < indexes.length; ++i) {
        indexes[i] = await this.__loadFromIndex(indexes[i], timeRange);
    }
    return indexes.reduce((a, b) => a.concat(b));
};
KibanaLoader.prototype.__requestToElsIndexes = async function(timeRange) {
    let requestObject = {
        "fields": ["@timestamp"],
        "index_constraints": {
            "@timestamp": {
                "max_value": {
                    "gte": timeRange.from,
                    "format": "epoch_millis"
                },
                "min_value": {
                    "lte": timeRange.to,
                    "format": "epoch_millis"
                }
            }
        }
    };

    let response = await Helper.sendPost("/elasticsearch/logstash-*/_field_stats?level=indices", JSON.stringify(requestObject));

    let indexes = Object.keys(response.indices);
    for (let i = 0; i < indexes.length; ++i) {
        let x = response.indices[indexes[i]].fields["@timestamp"];
        let indexTimeRange = new TimeRange(x.min_value, x.max_value);

        indexes[i] = new Index(indexes[i], indexTimeRange); // dirty..
    }

    return indexes;
};
KibanaLoader.prototype.__loadFromIndex = async function(index, timeRange) {
    timeRange = timeRange.getIntersection(index.timeRange);

    let data = await this.__requestToElsIndex(index.name, timeRange);
    if (data.total <= data.entities.length)
        return data.entities;

    let firstHalf = await this.__loadFromIndex(index, timeRange.getFirstHalf());
    let secondHalf = await this.__loadFromIndex(index, timeRange.getSecondHalf());
    return firstHalf.concat(secondHalf);
};
KibanaLoader.prototype.__requestToElsIndex = async function(indexName, timeRange) {
    let indexObject = { "index":[indexName], "ignore_unavailable":true };
    let queryObject = {
        "size": this.requestSize,
        "sort": [{
            "@timestamp": {
                "order": "desc",
                "unmapped_type": "boolean"
            }
        }],
        "query": {
            "filtered": {
                "query": {
                    "query_string": {
                        "query": this.queryString,
                        "analyze_wildcard": true,
                        "lowercase_expanded_terms": false
                    }
                },
                "filter": {
                    "bool": {
                        "must": [{
                            "range": {
                                "@timestamp": {
                                    "gte": timeRange.from,
                                    "lte": timeRange.to,
                                    "format": "epoch_millis"
                                }
                            }
                        }
                        ]
                    }
                }
            }
        },
        "fields": this.fields
    };
    let data = JSON.stringify(indexObject) + '\n'
             + JSON.stringify(queryObject) + '\n';

    console.log({indexName:indexName, from: timeRange.from, to: timeRange.to});

    let response = await Helper.reprocessAsync(
        () => Helper.sendPost("/elasticsearch/_msearch?timeout=0&ignore_unavailable=true", data),
        3,
        1000);

    let hits = response.responses[0].hits;
    let result = {total: hits.total, entities: hits.hits};

    console.log({total:result.total, count:result.entities.length});
    return result;
};