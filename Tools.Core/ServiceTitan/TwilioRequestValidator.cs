using System.Collections.Generic;
using Twilio.Security;

namespace Tools.Core.ServiceTitan
{
    public class TwilioRequestValidator
    {
        private readonly RequestValidator validator;

        public bool Validate(string url, string body, string expectedSignature)
        {
            if (string.IsNullOrEmpty(body)) {
                return validator.Validate(url, default(Dictionary<string, string>), expectedSignature);
            }

            var parameters = HttpValueCollection.ParseQueryString(body);
            return validator.Validate(url, parameters, expectedSignature);
        }

        public TwilioRequestValidator(string authToken)
        {
            validator = new RequestValidator(authToken);
        }
    }
}
