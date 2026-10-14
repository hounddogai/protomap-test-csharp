namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedRateMilestoneChecks
{
    public static void CappedRateMilestoneStartsInDraft()
    {
        var item = CappedRateMilestone.NewCappedRateMilestone("code-428", "label-654");
        Check.Equal(item.Status, CappedRateMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedRateMilestoneMovesToPending()
    {
        var item = CappedRateMilestone.NewCappedRateMilestone("code-428", "label-654");
        try
        {
            item.MoveTo(CappedRateMilestoneStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedRateMilestoneStatus.Pending, "status");
    }

    public static void CappedRateMilestoneRejectsMovesFromClosed()
    {
        var item = CappedRateMilestone.NewCappedRateMilestone("code-428", "label-654");
        item.Status = CappedRateMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedRateMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedRateMilestoneStartsInDraft();
        CappedRateMilestoneMovesToPending();
        CappedRateMilestoneRejectsMovesFromClosed();
    }
}
