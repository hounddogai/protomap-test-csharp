namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolWindowViewChecks
{
    public static void PoolWindowViewShowsTheStatusInCapitals()
    {
        var item = PoolWindow.NewPoolWindow("code-431", "title-338");
        var view = PoolWindowView.ToPoolWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolWindowView.ToPoolWindowViews(new List<PoolWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolWindowViewShowsTheStatusInCapitals();
    }
}
