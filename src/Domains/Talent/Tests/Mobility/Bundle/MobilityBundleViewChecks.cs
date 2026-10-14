namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityBundleViewChecks
{
    public static void MobilityBundleViewShowsTheStatusInCapitals()
    {
        var item = MobilityBundle.NewMobilityBundle("code-437", "title-898");
        var view = MobilityBundleView.ToMobilityBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityBundleView.ToMobilityBundleViews(new List<MobilityBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityBundleViewShowsTheStatusInCapitals();
    }
}
