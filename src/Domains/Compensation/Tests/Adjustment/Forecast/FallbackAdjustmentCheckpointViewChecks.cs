namespace Acme.Workforce.Compensation.Tests.Adjustment.Forecast;

using Acme.Workforce.Compensation.Adjustment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class FallbackAdjustmentCheckpointViewChecks
{
    public static void FallbackAdjustmentCheckpointViewShowsTheStatusInCapitals()
    {
        var item = FallbackAdjustmentCheckpoint.NewFallbackAdjustmentCheckpoint("code-117", "name-913");
        var view = FallbackAdjustmentCheckpointView.ToFallbackAdjustmentCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAdjustmentCheckpointView.ToFallbackAdjustmentCheckpointViews(new List<FallbackAdjustmentCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAdjustmentCheckpointViewShowsTheStatusInCapitals();
    }
}
