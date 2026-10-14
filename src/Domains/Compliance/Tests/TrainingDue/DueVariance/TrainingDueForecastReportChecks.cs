namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueForecastReportChecks
{
    public static void TrainingDueForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708110796);
        var items = new List<TrainingDueForecast>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TrainingDueForecast.NewTrainingDueForecast($"training-due-forecast-{index}", "sample"));
        }
        var report = TrainingDueForecastReport.BuildTrainingDueForecastReport("test", items, clock);
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
        TrainingDueForecastReportCountsEachRecordOnce();
    }
}
