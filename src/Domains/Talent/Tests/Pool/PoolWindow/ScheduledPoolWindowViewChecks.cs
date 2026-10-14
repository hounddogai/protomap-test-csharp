namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class ScheduledPoolWindowViewChecks
{
    public static void ScheduledPoolWindowViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("code-334", "title-570");
        var view = ScheduledPoolWindowView.ToScheduledPoolWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPoolWindowView.ToScheduledPoolWindowViews(new List<ScheduledPoolWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPoolWindowViewShowsTheStatusInCapitals();
    }
}
