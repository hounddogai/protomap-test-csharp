namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class BatchedPositionEstimateViewChecks
{
    public static void BatchedPositionEstimateViewShowsTheStatusInCapitals()
    {
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("code-742", "name-928");
        var view = BatchedPositionEstimateView.ToBatchedPositionEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedPositionEstimateView.ToBatchedPositionEstimateViews(new List<BatchedPositionEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedPositionEstimateViewShowsTheStatusInCapitals();
    }
}
