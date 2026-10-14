namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class DefaultRateDraftViewChecks
{
    public static void DefaultRateDraftViewShowsTheStatusInCapitals()
    {
        var item = DefaultRateDraft.NewDefaultRateDraft("code-837", "name-648");
        var view = DefaultRateDraftView.ToDefaultRateDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultRateDraftView.ToDefaultRateDraftViews(new List<DefaultRateDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultRateDraftViewShowsTheStatusInCapitals();
    }
}
