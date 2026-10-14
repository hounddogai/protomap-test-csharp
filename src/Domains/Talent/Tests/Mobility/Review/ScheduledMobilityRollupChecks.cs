namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledMobilityRollupChecks
{
    public static void ScheduledMobilityRollupStartsInNew()
    {
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("code-650", "title-287");
        Check.Equal(item.Status, ScheduledMobilityRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledMobilityRollupMovesToFailed()
    {
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("code-650", "title-287");
        try
        {
            item.MoveTo(ScheduledMobilityRollupStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledMobilityRollupStatus.Failed, "status");
    }

    public static void ScheduledMobilityRollupRejectsMovesFromFailed()
    {
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("code-650", "title-287");
        item.Status = ScheduledMobilityRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledMobilityRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledMobilityRollupStartsInNew();
        ScheduledMobilityRollupMovesToFailed();
        ScheduledMobilityRollupRejectsMovesFromFailed();
    }
}
