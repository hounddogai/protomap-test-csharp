namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingTrainingDueExceptionReportChecks
{
    public static void RollingTrainingDueExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706739333);
        var items = new List<RollingTrainingDueException>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingTrainingDueException.NewRollingTrainingDueException($"rolling-training-due-exception-{index}", "sample"));
        }
        var report = RollingTrainingDueExceptionReport.BuildRollingTrainingDueExceptionReport("test", items, clock);
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
        RollingTrainingDueExceptionReportCountsEachRecordOnce();
    }
}
