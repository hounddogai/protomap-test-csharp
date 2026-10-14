namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileCycleReportChecks
{
    public static void SkillProfileCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703281314);
        var items = new List<SkillProfileCycle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SkillProfileCycle.NewSkillProfileCycle($"skill-profile-cycle-{index}", "sample"));
        }
        var report = SkillProfileCycleReport.BuildSkillProfileCycleReport("test", items, clock);
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
        SkillProfileCycleReportCountsEachRecordOnce();
    }
}
