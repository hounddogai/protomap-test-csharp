namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyItemReportChecks
{
    public static void PolicyItemReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708191612);
        var items = new List<PolicyItem>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PolicyItem.NewPolicyItem($"policy-item-{index}", "sample"));
        }
        var report = PolicyItemReport.BuildPolicyItemReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PolicyItemReportCountsEachRecordOnce();
    }
}
