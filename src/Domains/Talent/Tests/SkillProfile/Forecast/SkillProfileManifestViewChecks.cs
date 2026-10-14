namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileManifestViewChecks
{
    public static void SkillProfileManifestViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileManifest.NewSkillProfileManifest("code-372", "label-72");
        var view = SkillProfileManifestView.ToSkillProfileManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileManifestView.ToSkillProfileManifestViews(new List<SkillProfileManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileManifestViewShowsTheStatusInCapitals();
    }
}
