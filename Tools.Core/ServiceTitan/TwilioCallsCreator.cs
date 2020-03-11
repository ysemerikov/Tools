using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using Twilio.Clients;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Tools.Core.ServiceTitan
{
    public class TwilioCallsCreator : IAction
    {
        private static readonly TwilioRestClient client = null;//new TwilioRestClient(Secrets.Twilio.ProductionUsername, Secrets.Twilio.ProductionPassword);
        private static readonly string[] callerIds = { "+14693004832", "+13125358366", "+15512307191", "+12062080992", "+16122301753", "+13053963809", "+17084983651", "+12407242932", "+13133273166", "+16506662228", "+12316385324", "+13025261936", "+14355654137", "+18038324219" };
        private static int callerIds_Counter = 0;

        private static readonly string[] calledIds = { "+13233647569", "+16502044814", "+13122489713", "+12063177749", "+16122259569", "+13052032008", "+12402038152", "+13132170771", "+16502094037", "+13433005764" };
        private static int calledIds_Counter = 0;

        public async Task Do()
        {
            return;
            var cps = 11;
            var callsAmount = 512;
            
            var tasks = new AsyncEnumerable<Task<string>>(async yield => {
                for (int i = 0; i < callsAmount; i++)
                {
                    await yield.ReturnAsync(CreateCall());

                    if (i > 0 && i % cps == 0) {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
            });

            var calls = await Task.WhenAll(await tasks.ToListAsync());
            File.AppendAllLines(@"C:\servicetitan\obcalls.txt", calls);
        }

        public async Task<string> CreateCall()
        {
            var r = Interlocked.Increment(ref callerIds_Counter) % callerIds.Length;
            var d = Interlocked.Increment(ref calledIds_Counter) % calledIds.Length;
            var from = callerIds[r];
            var to = calledIds[d];
            var call = await CreateCall(from, to);
            return $"{DateTime.UtcNow:s}\t{call.Sid}\t{call.From}\t{call.To}";
        }

        public Task<CallResource> CreateCall(string from, string to)
        {
//            to = $"sip:{to}@cust216.auth.bandwidth.com:5012;transport=udp?X-account-id=13264&X-account-token=cfa55a3c3f138b087a640d5b81aa2d978e8f3a1a";
            return CallResource.CreateAsync(new CreateCallOptions(new PhoneNumber(to), new PhoneNumber(from))
            {
                Method = HttpMethod.Post,
                Record = false,
                Url = new Uri("https://handler.twilio.com/twiml/EHecfe97c89ec5667fd441c57d88a16f25"),
//                SipAuthUsername = "st-production",
//                SipAuthPassword = "CfqNePG8cFWg",
            }, client);
        }
    }
}
