namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class PayRunCycleChecks
{
    public static void PayRunCycleStartsInIdle()
    {
        var item = PayRunCycle.NewPayRunCycle("code-153", "name-961");
        Check.Equal(item.Status, PayRunCycleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunCycleMovesToWarming()
    {
        var item = PayRunCycle.NewPayRunCycle("code-153", "name-961");
        try
        {
            item.MoveTo(PayRunCycleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunCycleStatus.Warming, "status");
    }

    public static void PayRunCycleRejectsMovesFromStopped()
    {
        var item = PayRunCycle.NewPayRunCycle("code-153", "name-961");
        item.Status = PayRunCycleStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayRunCycleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayRunCycleStartsInIdle();
        PayRunCycleMovesToWarming();
        PayRunCycleRejectsMovesFromStopped();
    }
}
