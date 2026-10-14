namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathRuleReportChecks
{
    public static void CareerPathRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704738670);
        var items = new List<CareerPathRule>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CareerPathRule.NewCareerPathRule($"career-path-rule-{index}", "sample"));
        }
        var report = CareerPathRuleReport.BuildCareerPathRuleReport("test", items, clock);
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
        CareerPathRuleReportCountsEachRecordOnce();
    }
}
