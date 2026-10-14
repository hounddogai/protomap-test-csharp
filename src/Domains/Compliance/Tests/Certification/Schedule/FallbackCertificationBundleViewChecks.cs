namespace Acme.Workforce.Compliance.Tests.Certification.Schedule;

using Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class FallbackCertificationBundleViewChecks
{
    public static void FallbackCertificationBundleViewShowsTheStatusInCapitals()
    {
        var item = FallbackCertificationBundle.NewFallbackCertificationBundle("code-999", "name-560");
        var view = FallbackCertificationBundleView.ToFallbackCertificationBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackCertificationBundleView.ToFallbackCertificationBundleViews(new List<FallbackCertificationBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackCertificationBundleViewShowsTheStatusInCapitals();
    }
}
