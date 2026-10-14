namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingManifestViewChecks
{
    public static void OffboardingManifestViewShowsTheStatusInCapitals()
    {
        var item = OffboardingManifest.NewOffboardingManifest("code-880", "title-220");
        var view = OffboardingManifestView.ToOffboardingManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingManifestView.ToOffboardingManifestViews(new List<OffboardingManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingManifestViewShowsTheStatusInCapitals();
    }
}
