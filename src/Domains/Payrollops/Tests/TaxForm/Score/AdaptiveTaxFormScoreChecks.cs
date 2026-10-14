namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveTaxFormScoreChecks
{
    public static void AdaptiveTaxFormScoreStartsInRequested()
    {
        var item = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore("code-152", "name-68");
        Check.Equal(item.Status, AdaptiveTaxFormScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTaxFormScoreMovesToExpired()
    {
        var item = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore("code-152", "name-68");
        try
        {
            item.MoveTo(AdaptiveTaxFormScoreStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTaxFormScoreStatus.Expired, "status");
    }

    public static void AdaptiveTaxFormScoreRejectsMovesFromExpired()
    {
        var item = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore("code-152", "name-68");
        item.Status = AdaptiveTaxFormScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveTaxFormScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormScoreStartsInRequested();
        AdaptiveTaxFormScoreMovesToExpired();
        AdaptiveTaxFormScoreRejectsMovesFromExpired();
    }
}
