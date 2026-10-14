namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeSegmentChecks
{
    public static void AccessBadgeSegmentStartsInCreated()
    {
        var item = AccessBadgeSegment.NewAccessBadgeSegment("code-891", "name-485");
        Check.Equal(item.Status, AccessBadgeSegmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeSegmentMovesToPublished()
    {
        var item = AccessBadgeSegment.NewAccessBadgeSegment("code-891", "name-485");
        try
        {
            item.MoveTo(AccessBadgeSegmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeSegmentStatus.Published, "status");
    }

    public static void AccessBadgeSegmentRejectsMovesFromRetired()
    {
        var item = AccessBadgeSegment.NewAccessBadgeSegment("code-891", "name-485");
        item.Status = AccessBadgeSegmentStatus.Retired;
        Check.Fails(() => item.MoveTo(AccessBadgeSegmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        AccessBadgeSegmentStartsInCreated();
        AccessBadgeSegmentMovesToPublished();
        AccessBadgeSegmentRejectsMovesFromRetired();
    }
}
