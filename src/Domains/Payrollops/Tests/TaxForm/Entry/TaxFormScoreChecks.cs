namespace Acme.Workforce.Payrollops.Tests.TaxForm.Entry;

using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TaxFormScoreChecks
{
    public static void TaxFormScoreStartsInNew()
    {
        var item = TaxFormScore.NewTaxFormScore("code-276", "label-659");
        Check.Equal(item.Status, TaxFormScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormScoreMovesToDone()
    {
        var item = TaxFormScore.NewTaxFormScore("code-276", "label-659");
        try
        {
            item.MoveTo(TaxFormScoreStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormScoreStatus.Done, "status");
    }

    public static void TaxFormScoreRejectsMovesFromFailed()
    {
        var item = TaxFormScore.NewTaxFormScore("code-276", "label-659");
        item.Status = TaxFormScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(TaxFormScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        TaxFormScoreStartsInNew();
        TaxFormScoreMovesToDone();
        TaxFormScoreRejectsMovesFromFailed();
    }
}
