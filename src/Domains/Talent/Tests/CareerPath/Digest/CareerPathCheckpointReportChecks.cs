namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CareerPathCheckpointReportChecks
{
    public static void CareerPathCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704107707);
        var items = new List<CareerPathCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CareerPathCheckpoint.NewCareerPathCheckpoint($"career-path-checkpoint-{index}", "sample"));
        }
        var report = CareerPathCheckpointReport.BuildCareerPathCheckpointReport("test", items, clock);
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
        CareerPathCheckpointReportCountsEachRecordOnce();
    }
}
