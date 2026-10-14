namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class RegionalPayRunGroupViewChecks
{
    public static void RegionalPayRunGroupViewShowsTheStatusInCapitals()
    {
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("code-923", "name-878");
        var view = RegionalPayRunGroupView.ToRegionalPayRunGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalPayRunGroupView.ToRegionalPayRunGroupViews(new List<RegionalPayRunGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalPayRunGroupViewShowsTheStatusInCapitals();
    }
}
