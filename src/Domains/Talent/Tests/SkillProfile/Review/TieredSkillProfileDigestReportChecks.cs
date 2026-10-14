namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredSkillProfileDigestReportChecks
{
    public static void TieredSkillProfileDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708466834);
        var items = new List<TieredSkillProfileDigest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredSkillProfileDigest.NewTieredSkillProfileDigest($"tiered-skill-profile-digest-{index}", "sample"));
        }
        var report = TieredSkillProfileDigestReport.BuildTieredSkillProfileDigestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TieredSkillProfileDigestReportCountsEachRecordOnce();
    }
}
