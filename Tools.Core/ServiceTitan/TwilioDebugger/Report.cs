namespace Tools.Core.ServiceTitan.TwilioDebugger
{
    public class Report
    {
        public string Sid { get; set; }
        public string CallSid { get; set; }
        public string ErrorCode { get; set; }
        public ErrorType ErrorType { get; set; }
        public string TenantPhone { get; set; }
    }

    public enum ErrorType
    {
        Unknown = 0,
        Other = 1,
        CallWasntConnected = 8,
        CallWasConnected = 9,
    }
}