namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class EquityGrantMappingViewChecks
{
    public static void EquityGrantMappingViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantMapping.NewEquityGrantMapping("code-746", "name-270");
        var view = EquityGrantMappingView.ToEquityGrantMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantMappingView.ToEquityGrantMappingViews(new List<EquityGrantMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantMappingViewShowsTheStatusInCapitals();
    }
}
