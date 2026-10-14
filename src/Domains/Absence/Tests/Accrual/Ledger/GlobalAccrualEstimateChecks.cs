namespace Acme.Workforce.Absence.Tests.Accrual.Ledger;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GlobalAccrualEstimateChecks
{
    public static void GlobalAccrualEstimateStartsInOpen()
    {
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("code-276", "name-925");
        Check.Equal(item.Status, GlobalAccrualEstimateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAccrualEstimateMovesToArchived()
    {
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("code-276", "name-925");
        try
        {
            item.MoveTo(GlobalAccrualEstimateStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAccrualEstimateStatus.Archived, "status");
    }

    public static void GlobalAccrualEstimateRejectsMovesFromArchived()
    {
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("code-276", "name-925");
        item.Status = GlobalAccrualEstimateStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalAccrualEstimateStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalAccrualEstimateStartsInOpen();
        GlobalAccrualEstimateMovesToArchived();
        GlobalAccrualEstimateRejectsMovesFromArchived();
    }
}
