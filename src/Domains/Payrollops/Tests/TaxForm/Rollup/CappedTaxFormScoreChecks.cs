namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedTaxFormScoreChecks
{
    public static void CappedTaxFormScoreStartsInNew()
    {
        var item = CappedTaxFormScore.NewCappedTaxFormScore("code-376", "title-645");
        Check.Equal(item.Status, CappedTaxFormScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedTaxFormScoreMovesToFailed()
    {
        var item = CappedTaxFormScore.NewCappedTaxFormScore("code-376", "title-645");
        try
        {
            item.MoveTo(CappedTaxFormScoreStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedTaxFormScoreStatus.Failed, "status");
    }

    public static void CappedTaxFormScoreRejectsMovesFromFailed()
    {
        var item = CappedTaxFormScore.NewCappedTaxFormScore("code-376", "title-645");
        item.Status = CappedTaxFormScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedTaxFormScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedTaxFormScoreStartsInNew();
        CappedTaxFormScoreMovesToFailed();
        CappedTaxFormScoreRejectsMovesFromFailed();
    }
}
