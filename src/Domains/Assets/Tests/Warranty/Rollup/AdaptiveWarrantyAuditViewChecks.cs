namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AdaptiveWarrantyAuditViewChecks
{
    public static void AdaptiveWarrantyAuditViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveWarrantyAudit.NewAdaptiveWarrantyAudit("code-152", "label-335");
        var view = AdaptiveWarrantyAuditView.ToAdaptiveWarrantyAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveWarrantyAuditView.ToAdaptiveWarrantyAuditViews(new List<AdaptiveWarrantyAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyAuditViewShowsTheStatusInCapitals();
    }
}
