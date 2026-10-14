namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyClaimViewChecks
{
    public static void WarrantyClaimViewShowsTheStatusInCapitals()
    {
        var item = WarrantyClaim.NewWarrantyClaim("code-400", "name-178");
        var view = WarrantyClaimView.ToWarrantyClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyClaimView.ToWarrantyClaimViews(new List<WarrantyClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyClaimViewShowsTheStatusInCapitals();
    }
}
