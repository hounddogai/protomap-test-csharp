namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientCareerPathRuleReportChecks
{
    public static void LenientCareerPathRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705839059);
        var items = new List<LenientCareerPathRule>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientCareerPathRule.NewLenientCareerPathRule($"lenient-career-path-rule-{index}", "sample"));
        }
        var report = LenientCareerPathRuleReport.BuildLenientCareerPathRuleReport("test", items, clock);
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
        LenientCareerPathRuleReportCountsEachRecordOnce();
    }
}
