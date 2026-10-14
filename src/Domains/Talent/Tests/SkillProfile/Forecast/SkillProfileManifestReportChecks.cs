namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileManifestReportChecks
{
    public static void SkillProfileManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705513987);
        var items = new List<SkillProfileManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SkillProfileManifest.NewSkillProfileManifest($"skill-profile-manifest-{index}", "sample"));
        }
        var report = SkillProfileManifestReport.BuildSkillProfileManifestReport("test", items, clock);
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
        SkillProfileManifestReportCountsEachRecordOnce();
    }
}
