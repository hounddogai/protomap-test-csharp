namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CostModelSummaryChecks
{
    public static void CostModelSummaryStartsInCreated()
    {
        var item = CostModelSummary.NewCostModelSummary("code-497", "label-626");
        Check.Equal(item.Status, CostModelSummaryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelSummaryMovesToPublished()
    {
        var item = CostModelSummary.NewCostModelSummary("code-497", "label-626");
        try
        {
            item.MoveTo(CostModelSummaryStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelSummaryStatus.Published, "status");
    }

    public static void CostModelSummaryRejectsMovesFromRetired()
    {
        var item = CostModelSummary.NewCostModelSummary("code-497", "label-626");
        item.Status = CostModelSummaryStatus.Retired;
        Check.Fails(() => item.MoveTo(CostModelSummaryStatus.Created), "move");
    }

    public static void RunAll()
    {
        CostModelSummaryStartsInCreated();
        CostModelSummaryMovesToPublished();
        CostModelSummaryRejectsMovesFromRetired();
    }
}
