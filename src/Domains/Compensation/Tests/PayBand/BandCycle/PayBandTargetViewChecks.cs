namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandTargetViewChecks
{
    public static void PayBandTargetViewShowsTheStatusInCapitals()
    {
        var item = PayBandTarget.NewPayBandTarget("code-551", "name-611");
        var view = PayBandTargetView.ToPayBandTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandTargetView.ToPayBandTargetViews(new List<PayBandTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandTargetViewShowsTheStatusInCapitals();
    }
}
