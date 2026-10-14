namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception checkpoint code, run by the project's runner.</summary>
public static class CappedExceptionCheckpointReportChecks
{
    public static void CappedExceptionCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705350608);
        var items = new List<CappedExceptionCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CappedExceptionCheckpoint.NewCappedExceptionCheckpoint($"capped-exception-checkpoint-{index}", "sample"));
        }
        var report = CappedExceptionCheckpointReport.BuildCappedExceptionCheckpointReport("test", items, clock);
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
        CappedExceptionCheckpointReportCountsEachRecordOnce();
    }
}
