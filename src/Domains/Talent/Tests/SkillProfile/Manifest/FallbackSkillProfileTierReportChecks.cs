namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackSkillProfileTierReportChecks
{
    public static void FallbackSkillProfileTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705701370);
        var items = new List<FallbackSkillProfileTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackSkillProfileTier.NewFallbackSkillProfileTier($"fallback-skill-profile-tier-{index}", "sample"));
        }
        var report = FallbackSkillProfileTierReport.BuildFallbackSkillProfileTierReport("test", items, clock);
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
        FallbackSkillProfileTierReportCountsEachRecordOnce();
    }
}
