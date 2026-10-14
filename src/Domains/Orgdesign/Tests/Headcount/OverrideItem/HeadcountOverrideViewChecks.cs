namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountOverrideViewChecks
{
    public static void HeadcountOverrideViewShowsTheStatusInCapitals()
    {
        var item = HeadcountOverride.NewHeadcountOverride("code-568", "label-117");
        var view = HeadcountOverrideView.ToHeadcountOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountOverrideView.ToHeadcountOverrideViews(new List<HeadcountOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountOverrideViewShowsTheStatusInCapitals();
    }
}
