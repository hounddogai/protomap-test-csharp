namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredSkillProfileCycleReportChecks
{
    public static void TieredSkillProfileCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703965580);
        var items = new List<TieredSkillProfileCycle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredSkillProfileCycle.NewTieredSkillProfileCycle($"tiered-skill-profile-cycle-{index}", "sample"));
        }
        var report = TieredSkillProfileCycleReport.BuildTieredSkillProfileCycleReport("test", items, clock);
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
        TieredSkillProfileCycleReportCountsEachRecordOnce();
    }
}
