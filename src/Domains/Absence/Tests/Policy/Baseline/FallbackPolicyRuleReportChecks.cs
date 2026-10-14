namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackPolicyRuleReportChecks
{
    public static void FallbackPolicyRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706398676);
        var items = new List<FallbackPolicyRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FallbackPolicyRule.NewFallbackPolicyRule($"fallback-policy-rule-{index}", "sample"));
        }
        var report = FallbackPolicyRuleReport.BuildFallbackPolicyRuleReport("test", items, clock);
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
        FallbackPolicyRuleReportCountsEachRecordOnce();
    }
}
