namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ManualAccessBadgeMilestoneChecks
{
    public static void ManualAccessBadgeMilestoneStartsInOpen()
    {
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("code-226", "title-148");
        Check.Equal(item.Status, ManualAccessBadgeMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAccessBadgeMilestoneMovesToArchived()
    {
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("code-226", "title-148");
        try
        {
            item.MoveTo(ManualAccessBadgeMilestoneStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAccessBadgeMilestoneStatus.Archived, "status");
    }

    public static void ManualAccessBadgeMilestoneRejectsMovesFromArchived()
    {
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("code-226", "title-148");
        item.Status = ManualAccessBadgeMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualAccessBadgeMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualAccessBadgeMilestoneStartsInOpen();
        ManualAccessBadgeMilestoneMovesToArchived();
        ManualAccessBadgeMilestoneRejectsMovesFromArchived();
    }
}
