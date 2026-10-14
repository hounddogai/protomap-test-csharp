namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfilePreferenceViewChecks
{
    public static void SkillProfilePreferenceViewShowsTheStatusInCapitals()
    {
        var item = SkillProfilePreference.NewSkillProfilePreference("code-523", "label-901");
        var view = SkillProfilePreferenceView.ToSkillProfilePreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfilePreferenceView.ToSkillProfilePreferenceViews(new List<SkillProfilePreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfilePreferenceViewShowsTheStatusInCapitals();
    }
}
