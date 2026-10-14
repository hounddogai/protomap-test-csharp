namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualCycleChecks
{
    public static void AccrualCycleStartsInNew()
    {
        var item = AccrualCycle.NewAccrualCycle("code-592", "name-662");
        Check.Equal(item.Status, AccrualCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualCycleMovesToRunning()
    {
        var item = AccrualCycle.NewAccrualCycle("code-592", "name-662");
        try
        {
            item.MoveTo(AccrualCycleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualCycleStatus.Running, "status");
    }

    public static void AccrualCycleRejectsMovesFromFailed()
    {
        var item = AccrualCycle.NewAccrualCycle("code-592", "name-662");
        item.Status = AccrualCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(AccrualCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        AccrualCycleStartsInNew();
        AccrualCycleMovesToRunning();
        AccrualCycleRejectsMovesFromFailed();
    }
}
