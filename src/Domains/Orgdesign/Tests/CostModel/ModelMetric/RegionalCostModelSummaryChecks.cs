namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class RegionalCostModelSummaryChecks
{
    public static void RegionalCostModelSummaryStartsInCreated()
    {
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("code-661", "title-450");
        Check.Equal(item.Status, RegionalCostModelSummaryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalCostModelSummaryMovesToRetired()
    {
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("code-661", "title-450");
        try
        {
            item.MoveTo(RegionalCostModelSummaryStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalCostModelSummaryStatus.Retired, "status");
    }

    public static void RegionalCostModelSummaryRejectsMovesFromRetired()
    {
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("code-661", "title-450");
        item.Status = RegionalCostModelSummaryStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalCostModelSummaryStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalCostModelSummaryStartsInCreated();
        RegionalCostModelSummaryMovesToRetired();
        RegionalCostModelSummaryRejectsMovesFromRetired();
    }
}
