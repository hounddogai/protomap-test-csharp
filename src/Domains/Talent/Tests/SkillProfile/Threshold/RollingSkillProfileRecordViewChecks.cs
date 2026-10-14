namespace Acme.Workforce.Talent.Tests.SkillProfile.Threshold;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RollingSkillProfileRecordViewChecks
{
    public static void RollingSkillProfileRecordViewShowsTheStatusInCapitals()
    {
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("code-256", "name-927");
        var view = RollingSkillProfileRecordView.ToRollingSkillProfileRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingSkillProfileRecordView.ToRollingSkillProfileRecordViews(new List<RollingSkillProfileRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingSkillProfileRecordViewShowsTheStatusInCapitals();
    }
}
