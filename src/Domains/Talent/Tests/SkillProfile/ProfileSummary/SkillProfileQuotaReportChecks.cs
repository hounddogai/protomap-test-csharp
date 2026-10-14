namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class SkillProfileQuotaReportChecks
{
    public static void SkillProfileQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705738229);
        var items = new List<SkillProfileQuota>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SkillProfileQuota.NewSkillProfileQuota($"skill-profile-quota-{index}", "sample"));
        }
        var report = SkillProfileQuotaReport.BuildSkillProfileQuotaReport("test", items, clock);
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
        SkillProfileQuotaReportCountsEachRecordOnce();
    }
}
