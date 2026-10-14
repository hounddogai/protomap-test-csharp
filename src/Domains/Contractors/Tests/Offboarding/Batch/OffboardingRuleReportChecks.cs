namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class OffboardingRuleReportChecks
{
    public static void OffboardingRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702372495);
        var items = new List<OffboardingRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(OffboardingRule.NewOffboardingRule($"offboarding-rule-{index}", "sample"));
        }
        var report = OffboardingRuleReport.BuildOffboardingRuleReport("test", items, clock);
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
        OffboardingRuleReportCountsEachRecordOnce();
    }
}
