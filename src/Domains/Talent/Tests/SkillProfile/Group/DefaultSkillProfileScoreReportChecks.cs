namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultSkillProfileScoreReportChecks
{
    public static void DefaultSkillProfileScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705819028);
        var items = new List<DefaultSkillProfileScore>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultSkillProfileScore.NewDefaultSkillProfileScore($"default-skill-profile-score-{index}", "sample"));
        }
        var report = DefaultSkillProfileScoreReport.BuildDefaultSkillProfileScoreReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultSkillProfileScoreReportCountsEachRecordOnce();
    }
}
