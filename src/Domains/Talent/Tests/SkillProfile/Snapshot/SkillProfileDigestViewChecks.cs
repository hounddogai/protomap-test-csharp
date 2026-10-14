namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfileDigestViewChecks
{
    public static void SkillProfileDigestViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileDigest.NewSkillProfileDigest("code-747", "title-935");
        var view = SkillProfileDigestView.ToSkillProfileDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileDigestView.ToSkillProfileDigestViews(new List<SkillProfileDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileDigestViewShowsTheStatusInCapitals();
    }
}
