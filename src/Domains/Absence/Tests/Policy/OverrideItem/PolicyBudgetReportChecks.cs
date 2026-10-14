namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyBudgetReportChecks
{
    public static void PolicyBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700984267);
        var items = new List<PolicyBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PolicyBudget.NewPolicyBudget($"policy-budget-{index}", "sample"));
        }
        var report = PolicyBudgetReport.BuildPolicyBudgetReport("test", items, clock);
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
        PolicyBudgetReportCountsEachRecordOnce();
    }
}
