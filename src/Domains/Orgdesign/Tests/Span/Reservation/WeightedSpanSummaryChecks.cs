namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedSpanSummaryChecks
{
    public static void WeightedSpanSummaryStartsInRequested()
    {
        var item = WeightedSpanSummary.NewWeightedSpanSummary("code-182", "title-261");
        Check.Equal(item.Status, WeightedSpanSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedSpanSummaryMovesToGranted()
    {
        var item = WeightedSpanSummary.NewWeightedSpanSummary("code-182", "title-261");
        try
        {
            item.MoveTo(WeightedSpanSummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedSpanSummaryStatus.Granted, "status");
    }

    public static void WeightedSpanSummaryRejectsMovesFromExpired()
    {
        var item = WeightedSpanSummary.NewWeightedSpanSummary("code-182", "title-261");
        item.Status = WeightedSpanSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedSpanSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedSpanSummaryStartsInRequested();
        WeightedSpanSummaryMovesToGranted();
        WeightedSpanSummaryRejectsMovesFromExpired();
    }
}
