namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedBalanceSummaryChecks
{
    public static void WeightedBalanceSummaryStartsInDraft()
    {
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("code-590", "label-598");
        Check.Equal(item.Status, WeightedBalanceSummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBalanceSummaryMovesToPending()
    {
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("code-590", "label-598");
        try
        {
            item.MoveTo(WeightedBalanceSummaryStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBalanceSummaryStatus.Pending, "status");
    }

    public static void WeightedBalanceSummaryRejectsMovesFromClosed()
    {
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("code-590", "label-598");
        item.Status = WeightedBalanceSummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedBalanceSummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedBalanceSummaryStartsInDraft();
        WeightedBalanceSummaryMovesToPending();
        WeightedBalanceSummaryRejectsMovesFromClosed();
    }
}
