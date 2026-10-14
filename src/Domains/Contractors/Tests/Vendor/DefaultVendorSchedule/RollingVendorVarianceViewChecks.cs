namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class RollingVendorVarianceViewChecks
{
    public static void RollingVendorVarianceViewShowsTheStatusInCapitals()
    {
        var item = RollingVendorVariance.NewRollingVendorVariance("code-223", "name-331");
        var view = RollingVendorVarianceView.ToRollingVendorVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingVendorVarianceView.ToRollingVendorVarianceViews(new List<RollingVendorVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingVendorVarianceViewShowsTheStatusInCapitals();
    }
}
