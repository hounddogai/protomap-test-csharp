namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class ManualCareerPathProfileViewChecks
{
    public static void ManualCareerPathProfileViewShowsTheStatusInCapitals()
    {
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("code-634", "name-428");
        var view = ManualCareerPathProfileView.ToManualCareerPathProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualCareerPathProfileView.ToManualCareerPathProfileViews(new List<ManualCareerPathProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualCareerPathProfileViewShowsTheStatusInCapitals();
    }
}
