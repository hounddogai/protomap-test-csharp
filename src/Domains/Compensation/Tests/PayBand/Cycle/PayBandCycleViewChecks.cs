namespace Acme.Workforce.Compensation.Tests.PayBand.Cycle;

using Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PayBandCycleViewChecks
{
    public static void PayBandCycleViewShowsTheStatusInCapitals()
    {
        var item = PayBandCycle.NewPayBandCycle("code-740", "name-954");
        var view = PayBandCycleView.ToPayBandCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandCycleView.ToPayBandCycleViews(new List<PayBandCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandCycleViewShowsTheStatusInCapitals();
    }
}
