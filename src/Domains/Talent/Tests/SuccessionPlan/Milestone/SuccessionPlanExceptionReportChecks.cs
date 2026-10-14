namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanExceptionReportChecks
{
    public static void SuccessionPlanExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700102908);
        var items = new List<SuccessionPlanException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SuccessionPlanException.NewSuccessionPlanException($"succession-plan-exception-{index}", "sample"));
        }
        var report = SuccessionPlanExceptionReport.BuildSuccessionPlanExceptionReport("test", items, clock);
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
        SuccessionPlanExceptionReportCountsEachRecordOnce();
    }
}
