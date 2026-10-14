namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class ScheduledAdjustmentRequestViewChecks
{
    public static void ScheduledAdjustmentRequestViewShowsTheStatusInCapitals()
    {
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("code-312", "label-378");
        var view = ScheduledAdjustmentRequestView.ToScheduledAdjustmentRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledAdjustmentRequestView.ToScheduledAdjustmentRequestViews(new List<ScheduledAdjustmentRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentRequestViewShowsTheStatusInCapitals();
    }
}
