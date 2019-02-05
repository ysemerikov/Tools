let __tab;
async function getTabAsync() {
    if (!__tab) {
        let resolve;
        let promise = new Promise(r => { resolve = r; });
        chrome.tabs.getSelected(null, tab => { resolve(tab); });
        __tab = await promise;
    }

    return __tab;
}

async function executeAsync(code) {
    let resolve;
    let promise = new Promise(r => { resolve = r; });
    let tab = await getTabAsync();
    // chrome.tabs.executeScript(tab.id, {file: 'main.js'}, x => {
    //     chrome.tabs.executeScript(tab.id, {code: code}, resolve);
    // });
    let action = code;
    chrome.tabs.executeScript(tab.id, {
        code: 'action = "' + code + '";' 
    }, function() {
        chrome.tabs.executeScript(tab.id, {file: 'main.js'});
    });

    return await promise;
}

function checkQueryAsync() {
    return executeAsync('3333');
}

function fetchAsync() {
    return executeAsync('oppa');
}

document.getElementById('checkQuery').addEventListener('click', checkQueryAsync);
document.getElementById('fetch').addEventListener('click', fetchAsync);