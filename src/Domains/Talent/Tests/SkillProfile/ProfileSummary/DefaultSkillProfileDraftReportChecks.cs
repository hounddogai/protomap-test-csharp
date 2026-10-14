namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class DefaultSkillProfileDraftReportChecks
{
    public static void DefaultSkillProfileDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704745813);
        var items = new List<DefaultSkillProfileDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultSkillProfileDraft.NewDefaultSkillProfileDraft($"default-skill-profile-draft-{index}", "sample"));
        }
        var report = DefaultSkillProfileDraftReport.BuildDefaultSkillProfileDraftReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultSkillProfileDraftReportCountsEachRecordOnce();
    }
}
