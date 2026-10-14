namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunCheckpointViewChecks
{
    public static void PayRunCheckpointViewShowsTheStatusInCapitals()
    {
        var item = PayRunCheckpoint.NewPayRunCheckpoint("code-528", "label-87");
        var view = PayRunCheckpointView.ToPayRunCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunCheckpointView.ToPayRunCheckpointViews(new List<PayRunCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunCheckpointViewShowsTheStatusInCapitals();
    }
}
