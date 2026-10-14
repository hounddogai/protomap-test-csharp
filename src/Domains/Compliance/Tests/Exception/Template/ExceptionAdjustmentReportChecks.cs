namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionAdjustmentReportChecks
{
    public static void ExceptionAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707004073);
        var items = new List<ExceptionAdjustment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ExceptionAdjustment.NewExceptionAdjustment($"exception-adjustment-{index}", "sample"));
        }
        var report = ExceptionAdjustmentReport.BuildExceptionAdjustmentReport("test", items, clock);
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
        ExceptionAdjustmentReportCountsEachRecordOnce();
    }
}
