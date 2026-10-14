namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAllocationChecks
{
    public static void AccrualAllocationStartsInRequested()
    {
        var item = AccrualAllocation.NewAccrualAllocation("code-479", "name-399");
        Check.Equal(item.Status, AccrualAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualAllocationMovesToGranted()
    {
        var item = AccrualAllocation.NewAccrualAllocation("code-479", "name-399");
        try
        {
            item.MoveTo(AccrualAllocationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualAllocationStatus.Granted, "status");
    }

    public static void AccrualAllocationRejectsMovesFromExpired()
    {
        var item = AccrualAllocation.NewAccrualAllocation("code-479", "name-399");
        item.Status = AccrualAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(AccrualAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccrualAllocationStartsInRequested();
        AccrualAllocationMovesToGranted();
        AccrualAllocationRejectsMovesFromExpired();
    }
}
