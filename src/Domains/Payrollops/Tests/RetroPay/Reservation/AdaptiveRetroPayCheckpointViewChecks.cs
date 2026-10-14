namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveRetroPayCheckpointViewChecks
{
    public static void AdaptiveRetroPayCheckpointViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("code-194", "name-767");
        var view = AdaptiveRetroPayCheckpointView.ToAdaptiveRetroPayCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveRetroPayCheckpointView.ToAdaptiveRetroPayCheckpointViews(new List<AdaptiveRetroPayCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveRetroPayCheckpointViewShowsTheStatusInCapitals();
    }
}
