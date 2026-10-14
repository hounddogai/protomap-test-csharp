namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedSkillProfileAdjustmentReportChecks
{
    public static void CappedSkillProfileAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703443749);
        var items = new List<CappedSkillProfileAdjustment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment($"capped-skill-profile-adjustment-{index}", "sample"));
        }
        var report = CappedSkillProfileAdjustmentReport.BuildCappedSkillProfileAdjustmentReport("test", items, clock);
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
        CappedSkillProfileAdjustmentReportCountsEachRecordOnce();
    }
}
