namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetRuleReportChecks
{
    public static void TimesheetRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708391321);
        var items = new List<TimesheetRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TimesheetRule.NewTimesheetRule($"timesheet-rule-{index}", "sample"));
        }
        var report = TimesheetRuleReport.BuildTimesheetRuleReport("test", items, clock);
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
        TimesheetRuleReportCountsEachRecordOnce();
    }
}
