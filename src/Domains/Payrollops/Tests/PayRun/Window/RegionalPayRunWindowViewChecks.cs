namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalPayRunWindowViewChecks
{
    public static void RegionalPayRunWindowViewShowsTheStatusInCapitals()
    {
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("code-496", "label-294");
        var view = RegionalPayRunWindowView.ToRegionalPayRunWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalPayRunWindowView.ToRegionalPayRunWindowViews(new List<RegionalPayRunWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalPayRunWindowViewShowsTheStatusInCapitals();
    }
}
