namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class LenientCostModelSummaryChecks
{
    public static void LenientCostModelSummaryStartsInRequested()
    {
        var item = LenientCostModelSummary.NewLenientCostModelSummary("code-309", "name-983");
        Check.Equal(item.Status, LenientCostModelSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientCostModelSummaryMovesToExpired()
    {
        var item = LenientCostModelSummary.NewLenientCostModelSummary("code-309", "name-983");
        try
        {
            item.MoveTo(LenientCostModelSummaryStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientCostModelSummaryStatus.Expired, "status");
    }

    public static void LenientCostModelSummaryRejectsMovesFromExpired()
    {
        var item = LenientCostModelSummary.NewLenientCostModelSummary("code-309", "name-983");
        item.Status = LenientCostModelSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientCostModelSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientCostModelSummaryStartsInRequested();
        LenientCostModelSummaryMovesToExpired();
        LenientCostModelSummaryRejectsMovesFromExpired();
    }
}
