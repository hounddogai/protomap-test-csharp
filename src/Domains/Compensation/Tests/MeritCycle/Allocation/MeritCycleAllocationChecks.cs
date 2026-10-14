namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleAllocationChecks
{
    public static void MeritCycleAllocationStartsInOpen()
    {
        var item = MeritCycleAllocation.NewMeritCycleAllocation("code-941", "title-869");
        Check.Equal(item.Status, MeritCycleAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleAllocationMovesToInReview()
    {
        var item = MeritCycleAllocation.NewMeritCycleAllocation("code-941", "title-869");
        try
        {
            item.MoveTo(MeritCycleAllocationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleAllocationStatus.InReview, "status");
    }

    public static void MeritCycleAllocationRejectsMovesFromArchived()
    {
        var item = MeritCycleAllocation.NewMeritCycleAllocation("code-941", "title-869");
        item.Status = MeritCycleAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(MeritCycleAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        MeritCycleAllocationStartsInOpen();
        MeritCycleAllocationMovesToInReview();
        MeritCycleAllocationRejectsMovesFromArchived();
    }
}
