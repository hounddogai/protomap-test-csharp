namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class LenientPayBandBundleViewChecks
{
    public static void LenientPayBandBundleViewShowsTheStatusInCapitals()
    {
        var item = LenientPayBandBundle.NewLenientPayBandBundle("code-54", "label-164");
        var view = LenientPayBandBundleView.ToLenientPayBandBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPayBandBundleView.ToLenientPayBandBundleViews(new List<LenientPayBandBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPayBandBundleViewShowsTheStatusInCapitals();
    }
}
