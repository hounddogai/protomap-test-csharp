namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class TieredPolicyRuleReportChecks
{
    public static void TieredPolicyRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709203297);
        var items = new List<TieredPolicyRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredPolicyRule.NewTieredPolicyRule($"tiered-policy-rule-{index}", "sample"));
        }
        var report = TieredPolicyRuleReport.BuildTieredPolicyRuleReport("test", items, clock);
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
        TieredPolicyRuleReportCountsEachRecordOnce();
    }
}
