namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ScheduledPolicyItemReportChecks
{
    public static void ScheduledPolicyItemReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705973658);
        var items = new List<ScheduledPolicyItem>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledPolicyItem.NewScheduledPolicyItem($"scheduled-policy-item-{index}", "sample"));
        }
        var report = ScheduledPolicyItemReport.BuildScheduledPolicyItemReport("test", items, clock);
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
        ScheduledPolicyItemReportCountsEachRecordOnce();
    }
}
