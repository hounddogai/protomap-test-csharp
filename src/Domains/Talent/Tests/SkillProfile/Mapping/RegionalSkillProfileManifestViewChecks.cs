namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class RegionalSkillProfileManifestViewChecks
{
    public static void RegionalSkillProfileManifestViewShowsTheStatusInCapitals()
    {
        var item = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest("code-201", "name-770");
        var view = RegionalSkillProfileManifestView.ToRegionalSkillProfileManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalSkillProfileManifestView.ToRegionalSkillProfileManifestViews(new List<RegionalSkillProfileManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalSkillProfileManifestViewShowsTheStatusInCapitals();
    }
}
