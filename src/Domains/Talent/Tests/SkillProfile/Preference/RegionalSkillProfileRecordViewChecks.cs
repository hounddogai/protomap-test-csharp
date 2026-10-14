namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalSkillProfileRecordViewChecks
{
    public static void RegionalSkillProfileRecordViewShowsTheStatusInCapitals()
    {
        var item = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord("code-250", "label-318");
        var view = RegionalSkillProfileRecordView.ToRegionalSkillProfileRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalSkillProfileRecordView.ToRegionalSkillProfileRecordViews(new List<RegionalSkillProfileRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalSkillProfileRecordViewShowsTheStatusInCapitals();
    }
}
