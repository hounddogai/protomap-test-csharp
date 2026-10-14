namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientVisitorCheckpointReportChecks
{
    public static void LenientVisitorCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707229762);
        var items = new List<LenientVisitorCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientVisitorCheckpoint.NewLenientVisitorCheckpoint($"lenient-visitor-checkpoint-{index}", "sample"));
        }
        var report = LenientVisitorCheckpointReport.BuildLenientVisitorCheckpointReport("test", items, clock);
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
        LenientVisitorCheckpointReportCountsEachRecordOnce();
    }
}
