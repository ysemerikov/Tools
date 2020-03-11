using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace Tools.Core.ServiceTitan.TwilioDebugger
{
    public class TwilioLogic
    {
        private static readonly string[] otherErrorCodes = new[] {"32014", "32011", "32102", "13224"};

        public static async Task<Report> GetReport(NotificationResource @event, TwilioRestClient client)
        {
            var report = new Report {Sid = @event.Sid, ErrorCode = @event.ErrorCode};
            if (otherErrorCodes.Contains(@event.ErrorCode))
            {
                report.ErrorType = ErrorType.Other;
                return report;
            }
            report.CallSid = @event.CallSid;

            var url = GetUrl(@event);
            if (url == null)
                throw new ArgumentException($"Can't determine url for {@event.Sid}");

            if (url.EndsWith("/InboundCallDialStatusChange")
            || url.EndsWith("/OutboundCallDialStatusChange")
            || url.EndsWith("/OutboundCallConnected") // not necessary for call connection callback
            || url.EndsWith("/InboundCallStatus")
            || url.EndsWith("/OutboundCallStatus")
            )
            {
                report.ErrorType = ErrorType.CallWasConnected;
                return report;
            }

            if (url.EndsWith("/Tulsa"))
            {
                report.ErrorType = ErrorType.CallWasntConnected;
                report.TenantPhone = "tulsa";
                return report;
            }

            if (url.EndsWith("/Okc"))
            {
                report.ErrorType = ErrorType.CallWasntConnected;
                report.TenantPhone = "okc";
                return report;
            }

            if (url.EndsWith("/Index"))
            {
                var msg = GetPartOf(@event.MessageText, "Msg");
                if (
                    msg?.StartsWith(
                        "HTTP+Connection+Failure+-+Read+timed+out.+Falling+back+to+http%3A%2F%2Ftwimlets.com%2Fforward%3FPhoneNumber%3D%25") ==
                    true
                    ||
                    msg?.StartsWith(
                        "An+attempt+to+retrieve+content+from+https%3A%2F%2Ftwilio.servicetitan.com%2FTwilioProxy%2FIndex+returned+the+HTTP+status+code+502.+Falling+back+to+http%3A%2F%2Ftwimlets.com%2Fforward%3FPhoneNumber%3D%25") ==
                    true
                    ||
                    msg?.StartsWith(
                        "An+attempt+to+retrieve+content+from+https%3A%2F%2Ftwilio.servicetitan.com%2FTwilioProxy%2FIndex+returned+the+HTTP+status+code+502.+Falling+back+to+https%3A%2F%2Fignorant-goat-7596.twil.io") ==
                    true
                    ||
                    msg?.StartsWith(
                        "HTTP+Connection+Failure+-+Read+timed+out.+Falling+back+to+https%3A%2F%2Fignorant-goat-7596.twil.io") ==
                    true
                    ||
                    GetPartOf(@event.MessageText, "msg")?.StartsWith(
                        "HTTP+Connection+Failure+-+Read+timed+out.+Falling+back+to+http%3A%2F%2Ftwimlets.com%2Fforward%3FPhoneNumber%3D%25") ==
                    true
                )
                {
                    report.ErrorType = ErrorType.CallWasConnected;
                    return report;
                }
            }

            var call = await CallResource.FetchAsync(@event.CallSid, client: client);

            if (url.EndsWith("/OutboundCallGather"))
            {
                report.ErrorType = ErrorType.CallWasntConnected;
                report.TenantPhone = call.From;
            } else if (url.EndsWith("/OutboundCallResponse"))
            {
                if (await HasChildren(call, client))
                    throw new NotSupportedException($"OutboundCallResponse - {@event.Sid}");

                report.ErrorType = ErrorType.CallWasntConnected;
                report.TenantPhone = call.From;
            } else if (url.EndsWith("/Index"))
            {
                if (await HasChildren(call, client))
                    throw new NotSupportedException($"Index - {@event.Sid}");

                report.ErrorType = ErrorType.CallWasntConnected;
                report.TenantPhone = call.To;
            }

            return report;
        }

        private static async Task<bool> HasChildren(CallResource call, TwilioRestClient client)
        {
            if (!string.IsNullOrEmpty(call.ParentCallSid))
                throw new ArgumentException($"Child call {call.Sid} isn't expected.");

            var children = await CallResource.ReadAsync(new ReadCallOptions {ParentCallSid = call.Sid}, client);
            return children.Any();
        }

        private static string GetUrl(NotificationResource @event)
        {
            var url = @event.RequestUrl?.ToString()
                      ?? HttpUtility.UrlDecode(GetPartOf(@event.MessageText, "url"));
            url = url?.Replace(":443", "").Split('?').FirstOrDefault();
            if (url == null)
                throw new Exception(@event.MessageText);

            var prefix = "https://twilio.servicetitan.com/TwilioProxy/";
            if (!url.StartsWith(prefix))
                throw new NotSupportedException(url);

            return url;
        }

        private static string GetPartOf(string messageText, string name)
        {
            var prefix = $"{name}=";
            return messageText
                ?.Split('&')
                .FirstOrDefault(x => x.StartsWith(prefix))
                ?.Substring(prefix.Length);
        }
    }
}