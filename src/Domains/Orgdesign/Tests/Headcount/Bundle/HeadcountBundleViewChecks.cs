namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class HeadcountBundleViewChecks
{
    public static void HeadcountBundleViewShowsTheStatusInCapitals()
    {
        var item = HeadcountBundle.NewHeadcountBundle("code-460", "label-92");
        var view = HeadcountBundleView.ToHeadcountBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountBundleView.ToHeadcountBundleViews(new List<HeadcountBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountBundleViewShowsTheStatusInCapitals();
    }
}
