namespace Acme.Workforce.Orgdesign.Tests.CostModel.Variance;

using Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class FallbackCostModelLedgerChecks
{
    public static void FallbackCostModelLedgerStartsInIdle()
    {
        var item = FallbackCostModelLedger.NewFallbackCostModelLedger("code-774", "title-850");
        Check.Equal(item.Status, FallbackCostModelLedgerStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCostModelLedgerMovesToReady()
    {
        var item = FallbackCostModelLedger.NewFallbackCostModelLedger("code-774", "title-850");
        try
        {
            item.MoveTo(FallbackCostModelLedgerStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCostModelLedgerStatus.Ready, "status");
    }

    public static void FallbackCostModelLedgerRejectsMovesFromStopped()
    {
        var item = FallbackCostModelLedger.NewFallbackCostModelLedger("code-774", "title-850");
        item.Status = FallbackCostModelLedgerStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackCostModelLedgerStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackCostModelLedgerStartsInIdle();
        FallbackCostModelLedgerMovesToReady();
        FallbackCostModelLedgerRejectsMovesFromStopped();
    }
}
