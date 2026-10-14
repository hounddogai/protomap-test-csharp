namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueExceptionReportChecks
{
    public static void AdaptiveTrainingDueExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708989404);
        var items = new List<AdaptiveTrainingDueException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveTrainingDueException.NewAdaptiveTrainingDueException($"adaptive-training-due-exception-{index}", "sample"));
        }
        var report = AdaptiveTrainingDueExceptionReport.BuildAdaptiveTrainingDueExceptionReport("test", items, clock);
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
        AdaptiveTrainingDueExceptionReportCountsEachRecordOnce();
    }
}
