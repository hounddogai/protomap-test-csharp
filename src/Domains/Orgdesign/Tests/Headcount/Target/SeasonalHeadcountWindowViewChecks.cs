namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalHeadcountWindowViewChecks
{
    public static void SeasonalHeadcountWindowViewShowsTheStatusInCapitals()
    {
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("code-302", "label-369");
        var view = SeasonalHeadcountWindowView.ToSeasonalHeadcountWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalHeadcountWindowView.ToSeasonalHeadcountWindowViews(new List<SeasonalHeadcountWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalHeadcountWindowViewShowsTheStatusInCapitals();
    }
}
