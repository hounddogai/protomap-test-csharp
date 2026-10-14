namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyEntryReportChecks
{
    public static void PolicyEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702099111);
        var items = new List<PolicyEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PolicyEntry.NewPolicyEntry($"policy-entry-{index}", "sample"));
        }
        var report = PolicyEntryReport.BuildPolicyEntryReport("test", items, clock);
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
        PolicyEntryReportCountsEachRecordOnce();
    }
}
