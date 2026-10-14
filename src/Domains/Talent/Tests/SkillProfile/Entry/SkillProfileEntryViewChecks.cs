namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileEntryViewChecks
{
    public static void SkillProfileEntryViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileEntry.NewSkillProfileEntry("code-929", "label-613");
        var view = SkillProfileEntryView.ToSkillProfileEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileEntryView.ToSkillProfileEntryViews(new List<SkillProfileEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileEntryViewShowsTheStatusInCapitals();
    }
}
