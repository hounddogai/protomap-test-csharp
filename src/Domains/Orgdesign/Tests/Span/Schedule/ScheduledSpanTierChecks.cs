namespace Acme.Workforce.Orgdesign.Tests.Span.Schedule;

using Acme.Workforce.Orgdesign.Span.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledSpanTierChecks
{
    public static void ScheduledSpanTierStartsInRequested()
    {
        var item = ScheduledSpanTier.NewScheduledSpanTier("code-491", "title-957");
        Check.Equal(item.Status, ScheduledSpanTierStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledSpanTierMovesToExpired()
    {
        var item = ScheduledSpanTier.NewScheduledSpanTier("code-491", "title-957");
        try
        {
            item.MoveTo(ScheduledSpanTierStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledSpanTierStatus.Expired, "status");
    }

    public static void ScheduledSpanTierRejectsMovesFromExpired()
    {
        var item = ScheduledSpanTier.NewScheduledSpanTier("code-491", "title-957");
        item.Status = ScheduledSpanTierStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledSpanTierStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledSpanTierStartsInRequested();
        ScheduledSpanTierMovesToExpired();
        ScheduledSpanTierRejectsMovesFromExpired();
    }
}
