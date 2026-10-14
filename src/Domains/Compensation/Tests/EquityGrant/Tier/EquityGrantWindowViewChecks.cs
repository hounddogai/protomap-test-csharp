namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantWindowViewChecks
{
    public static void EquityGrantWindowViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantWindow.NewEquityGrantWindow("code-734", "label-585");
        var view = EquityGrantWindowView.ToEquityGrantWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantWindowView.ToEquityGrantWindowViews(new List<EquityGrantWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantWindowViewShowsTheStatusInCapitals();
    }
}
