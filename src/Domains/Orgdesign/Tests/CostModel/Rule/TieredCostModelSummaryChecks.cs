namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelSummaryChecks
{
    public static void TieredCostModelSummaryStartsInRequested()
    {
        var item = TieredCostModelSummary.NewTieredCostModelSummary("code-351", "title-199");
        Check.Equal(item.Status, TieredCostModelSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredCostModelSummaryMovesToGranted()
    {
        var item = TieredCostModelSummary.NewTieredCostModelSummary("code-351", "title-199");
        try
        {
            item.MoveTo(TieredCostModelSummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredCostModelSummaryStatus.Granted, "status");
    }

    public static void TieredCostModelSummaryRejectsMovesFromExpired()
    {
        var item = TieredCostModelSummary.NewTieredCostModelSummary("code-351", "title-199");
        item.Status = TieredCostModelSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredCostModelSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredCostModelSummaryStartsInRequested();
        TieredCostModelSummaryMovesToGranted();
        TieredCostModelSummaryRejectsMovesFromExpired();
    }
}
