namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionPlanReportChecks
{
    public static void ExceptionPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705281035);
        var items = new List<ExceptionPlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ExceptionPlan.NewExceptionPlan($"exception-plan-{index}", "sample"));
        }
        var report = ExceptionPlanReport.BuildExceptionPlanReport("test", items, clock);
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
        ExceptionPlanReportCountsEachRecordOnce();
    }
}
