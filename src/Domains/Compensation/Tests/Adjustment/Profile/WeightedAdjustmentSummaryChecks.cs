namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedAdjustmentSummaryChecks
{
    public static void WeightedAdjustmentSummaryStartsInRequested()
    {
        var item = WeightedAdjustmentSummary.NewWeightedAdjustmentSummary("code-833", "label-490");
        Check.Equal(item.Status, WeightedAdjustmentSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAdjustmentSummaryMovesToGranted()
    {
        var item = WeightedAdjustmentSummary.NewWeightedAdjustmentSummary("code-833", "label-490");
        try
        {
            item.MoveTo(WeightedAdjustmentSummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAdjustmentSummaryStatus.Granted, "status");
    }

    public static void WeightedAdjustmentSummaryRejectsMovesFromExpired()
    {
        var item = WeightedAdjustmentSummary.NewWeightedAdjustmentSummary("code-833", "label-490");
        item.Status = WeightedAdjustmentSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedAdjustmentSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedAdjustmentSummaryStartsInRequested();
        WeightedAdjustmentSummaryMovesToGranted();
        WeightedAdjustmentSummaryRejectsMovesFromExpired();
    }
}
