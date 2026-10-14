namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementSegmentChecks
{
    public static void EngagementSegmentStartsInRequested()
    {
        var item = EngagementSegment.NewEngagementSegment("code-241", "title-193");
        Check.Equal(item.Status, EngagementSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementSegmentMovesToExpired()
    {
        var item = EngagementSegment.NewEngagementSegment("code-241", "title-193");
        try
        {
            item.MoveTo(EngagementSegmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementSegmentStatus.Expired, "status");
    }

    public static void EngagementSegmentRejectsMovesFromExpired()
    {
        var item = EngagementSegment.NewEngagementSegment("code-241", "title-193");
        item.Status = EngagementSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(EngagementSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        EngagementSegmentStartsInRequested();
        EngagementSegmentMovesToExpired();
        EngagementSegmentRejectsMovesFromExpired();
    }
}
