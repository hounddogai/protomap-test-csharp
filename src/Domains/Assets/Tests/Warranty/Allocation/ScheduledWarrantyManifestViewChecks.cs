namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ScheduledWarrantyManifestViewChecks
{
    public static void ScheduledWarrantyManifestViewShowsTheStatusInCapitals()
    {
        var item = ScheduledWarrantyManifest.NewScheduledWarrantyManifest("code-999", "label-483");
        var view = ScheduledWarrantyManifestView.ToScheduledWarrantyManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledWarrantyManifestView.ToScheduledWarrantyManifestViews(new List<ScheduledWarrantyManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledWarrantyManifestViewShowsTheStatusInCapitals();
    }
}
