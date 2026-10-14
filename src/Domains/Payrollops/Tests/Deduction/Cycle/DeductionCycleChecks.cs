namespace Acme.Workforce.Payrollops.Tests.Deduction.Cycle;

using Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DeductionCycleChecks
{
    public static void DeductionCycleStartsInRequested()
    {
        var item = DeductionCycle.NewDeductionCycle("code-764", "name-510");
        Check.Equal(item.Status, DeductionCycleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionCycleMovesToGranted()
    {
        var item = DeductionCycle.NewDeductionCycle("code-764", "name-510");
        try
        {
            item.MoveTo(DeductionCycleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionCycleStatus.Granted, "status");
    }

    public static void DeductionCycleRejectsMovesFromExpired()
    {
        var item = DeductionCycle.NewDeductionCycle("code-764", "name-510");
        item.Status = DeductionCycleStatus.Expired;
        Check.Fails(() => item.MoveTo(DeductionCycleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DeductionCycleStartsInRequested();
        DeductionCycleMovesToGranted();
        DeductionCycleRejectsMovesFromExpired();
    }
}
