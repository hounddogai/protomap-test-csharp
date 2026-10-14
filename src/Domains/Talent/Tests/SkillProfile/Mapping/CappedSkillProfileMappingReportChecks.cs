namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class CappedSkillProfileMappingReportChecks
{
    public static void CappedSkillProfileMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707422762);
        var items = new List<CappedSkillProfileMapping>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedSkillProfileMapping.NewCappedSkillProfileMapping($"capped-skill-profile-mapping-{index}", "sample"));
        }
        var report = CappedSkillProfileMappingReport.BuildCappedSkillProfileMappingReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedSkillProfileMappingReportCountsEachRecordOnce();
    }
}
