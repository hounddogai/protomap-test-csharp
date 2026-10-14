namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingDeskMilestoneChecks
{
    public static void PendingDeskMilestoneStartsInDraft()
    {
        var item = PendingDeskMilestone.NewPendingDeskMilestone("code-591", "name-221");
        Check.Equal(item.Status, PendingDeskMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingDeskMilestoneMovesToActive()
    {
        var item = PendingDeskMilestone.NewPendingDeskMilestone("code-591", "name-221");
        try
        {
            item.MoveTo(PendingDeskMilestoneStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingDeskMilestoneStatus.Active, "status");
    }

    public static void PendingDeskMilestoneRejectsMovesFromClosed()
    {
        var item = PendingDeskMilestone.NewPendingDeskMilestone("code-591", "name-221");
        item.Status = PendingDeskMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingDeskMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingDeskMilestoneStartsInDraft();
        PendingDeskMilestoneMovesToActive();
        PendingDeskMilestoneRejectsMovesFromClosed();
    }
}
