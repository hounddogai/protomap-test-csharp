namespace Acme.Workforce.Compensation.Tests.MeritCycle.Batch;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryMeritCycleAllocationChecks
{
    public static void PrimaryMeritCycleAllocationStartsInPlanned()
    {
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("code-380", "title-682");
        Check.Equal(item.Status, PrimaryMeritCycleAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryMeritCycleAllocationMovesToInProgress()
    {
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("code-380", "title-682");
        try
        {
            item.MoveTo(PrimaryMeritCycleAllocationStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryMeritCycleAllocationStatus.InProgress, "status");
    }

    public static void PrimaryMeritCycleAllocationRejectsMovesFromCancelled()
    {
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("code-380", "title-682");
        item.Status = PrimaryMeritCycleAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryMeritCycleAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleAllocationStartsInPlanned();
        PrimaryMeritCycleAllocationMovesToInProgress();
        PrimaryMeritCycleAllocationRejectsMovesFromCancelled();
    }
}
