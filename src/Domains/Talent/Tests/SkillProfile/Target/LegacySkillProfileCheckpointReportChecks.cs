namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacySkillProfileCheckpointReportChecks
{
    public static void LegacySkillProfileCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703354069);
        var items = new List<LegacySkillProfileCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacySkillProfileCheckpoint.NewLegacySkillProfileCheckpoint($"legacy-skill-profile-checkpoint-{index}", "sample"));
        }
        var report = LegacySkillProfileCheckpointReport.BuildLegacySkillProfileCheckpointReport("test", items, clock);
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
        LegacySkillProfileCheckpointReportCountsEachRecordOnce();
    }
}
