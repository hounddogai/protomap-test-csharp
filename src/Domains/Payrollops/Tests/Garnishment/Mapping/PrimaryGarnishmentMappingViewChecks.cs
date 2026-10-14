namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryGarnishmentMappingViewChecks
{
    public static void PrimaryGarnishmentMappingViewShowsTheStatusInCapitals()
    {
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("code-519", "name-206");
        var view = PrimaryGarnishmentMappingView.ToPrimaryGarnishmentMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryGarnishmentMappingView.ToPrimaryGarnishmentMappingViews(new List<PrimaryGarnishmentMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryGarnishmentMappingViewShowsTheStatusInCapitals();
    }
}
