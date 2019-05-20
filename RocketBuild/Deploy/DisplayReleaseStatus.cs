using System;

namespace RocketBuild.Deploy
{
    [Flags]
    public enum DisplayReleaseStatus
    {
        Undefined = 0,
        NotStarted = 1,
        InProgress = 2,
        Succeeded = 4,
        Canceled = 8,
        Rejected = 16,
        Queued = 32,
        Scheduled = 64,
        PartiallySucceeded = 128
    }
}