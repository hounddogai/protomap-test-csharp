namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredEquityGrantWindowViewChecks
{
    public static void TieredEquityGrantWindowViewShowsTheStatusInCapitals()
    {
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("code-401", "name-199");
        var view = TieredEquityGrantWindowView.ToTieredEquityGrantWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredEquityGrantWindowView.ToTieredEquityGrantWindowViews(new List<TieredEquityGrantWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredEquityGrantWindowViewShowsTheStatusInCapitals();
    }
}
