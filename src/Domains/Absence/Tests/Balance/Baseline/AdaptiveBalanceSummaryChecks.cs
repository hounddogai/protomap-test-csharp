namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceSummaryChecks
{
    public static void AdaptiveBalanceSummaryStartsInRequested()
    {
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("code-456", "label-781");
        Check.Equal(item.Status, AdaptiveBalanceSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveBalanceSummaryMovesToGranted()
    {
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("code-456", "label-781");
        try
        {
            item.MoveTo(AdaptiveBalanceSummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveBalanceSummaryStatus.Granted, "status");
    }

    public static void AdaptiveBalanceSummaryRejectsMovesFromExpired()
    {
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("code-456", "label-781");
        item.Status = AdaptiveBalanceSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveBalanceSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveBalanceSummaryStartsInRequested();
        AdaptiveBalanceSummaryMovesToGranted();
        AdaptiveBalanceSummaryRejectsMovesFromExpired();
    }
}
