namespace PostsAPI.Options
{
    public class RequestTimingOptions
    {
        public const string RequestTiming = "RequestTiming";

        public int SlowRequestThresholdMs { get; set; } = 500;
    }
}
