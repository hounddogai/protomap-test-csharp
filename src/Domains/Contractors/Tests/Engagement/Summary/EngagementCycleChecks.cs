namespace Acme.Workforce.Contractors.Tests.Engagement.Summary;

using Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EngagementCycleChecks
{
    public static void EngagementCycleStartsInOpen()
    {
        var item = EngagementCycle.NewEngagementCycle("code-510", "label-95");
        Check.Equal(item.Status, EngagementCycleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementCycleMovesToReopened()
    {
        var item = EngagementCycle.NewEngagementCycle("code-510", "label-95");
        try
        {
            item.MoveTo(EngagementCycleStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementCycleStatus.Reopened, "status");
    }

    public static void EngagementCycleRejectsMovesFromArchived()
    {
        var item = EngagementCycle.NewEngagementCycle("code-510", "label-95");
        item.Status = EngagementCycleStatus.Archived;
        Check.Fails(() => item.MoveTo(EngagementCycleStatus.Open), "move");
    }

    public static void RunAll()
    {
        EngagementCycleStartsInOpen();
        EngagementCycleMovesToReopened();
        EngagementCycleRejectsMovesFromArchived();
    }
}
