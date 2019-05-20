namespace RocketBuild.Build
{
    public enum BuildResult
    {
        None = 0,
        Succeeded = 2,
        PartiallySucceeded = 4,
        Failed = 8,
        Canceled = 32
    }
}