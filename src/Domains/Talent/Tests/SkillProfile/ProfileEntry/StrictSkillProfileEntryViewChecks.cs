namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileEntry;

using Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>Checks of profile entry code, run by the project's runner.</summary>
public static class StrictSkillProfileEntryViewChecks
{
    public static void StrictSkillProfileEntryViewShowsTheStatusInCapitals()
    {
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("code-382", "title-180");
        var view = StrictSkillProfileEntryView.ToStrictSkillProfileEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictSkillProfileEntryView.ToStrictSkillProfileEntryViews(new List<StrictSkillProfileEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictSkillProfileEntryViewShowsTheStatusInCapitals();
    }
}
