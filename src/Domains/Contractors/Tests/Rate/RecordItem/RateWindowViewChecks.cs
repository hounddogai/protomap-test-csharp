namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateWindowViewChecks
{
    public static void RateWindowViewShowsTheStatusInCapitals()
    {
        var item = RateWindow.NewRateWindow("code-895", "title-850");
        var view = RateWindowView.ToRateWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateWindowView.ToRateWindowViews(new List<RateWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateWindowViewShowsTheStatusInCapitals();
    }
}
