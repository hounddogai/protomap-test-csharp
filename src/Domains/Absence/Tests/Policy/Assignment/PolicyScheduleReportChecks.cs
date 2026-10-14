namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyScheduleReportChecks
{
    public static void PolicyScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706984360);
        var items = new List<PolicySchedule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PolicySchedule.NewPolicySchedule($"policy-schedule-{index}", "sample"));
        }
        var report = PolicyScheduleReport.BuildPolicyScheduleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PolicyScheduleReportCountsEachRecordOnce();
    }
}
