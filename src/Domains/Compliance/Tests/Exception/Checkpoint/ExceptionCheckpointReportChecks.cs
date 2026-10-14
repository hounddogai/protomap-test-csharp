namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionCheckpointReportChecks
{
    public static void ExceptionCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709263375);
        var items = new List<ExceptionCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ExceptionCheckpoint.NewExceptionCheckpoint($"exception-checkpoint-{index}", "sample"));
        }
        var report = ExceptionCheckpointReport.BuildExceptionCheckpointReport("test", items, clock);
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
        ExceptionCheckpointReportCountsEachRecordOnce();
    }
}
