namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class SkillProfileSlotReportChecks
{
    public static void SkillProfileSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705444115);
        var items = new List<SkillProfileSlot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SkillProfileSlot.NewSkillProfileSlot($"skill-profile-slot-{index}", "sample"));
        }
        var report = SkillProfileSlotReport.BuildSkillProfileSlotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SkillProfileSlotReportCountsEachRecordOnce();
    }
}
