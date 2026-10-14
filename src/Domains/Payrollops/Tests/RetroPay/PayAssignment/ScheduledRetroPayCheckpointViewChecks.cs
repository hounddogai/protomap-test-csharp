namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class ScheduledRetroPayCheckpointViewChecks
{
    public static void ScheduledRetroPayCheckpointViewShowsTheStatusInCapitals()
    {
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("code-164", "label-438");
        var view = ScheduledRetroPayCheckpointView.ToScheduledRetroPayCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledRetroPayCheckpointView.ToScheduledRetroPayCheckpointViews(new List<ScheduledRetroPayCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledRetroPayCheckpointViewShowsTheStatusInCapitals();
    }
}
