namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class BatchedMobilityProfileReportChecks
{
    public static void BatchedMobilityProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704868349);
        var items = new List<BatchedMobilityProfile>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BatchedMobilityProfile.NewBatchedMobilityProfile($"batched-mobility-profile-{index}", "sample"));
        }
        var report = BatchedMobilityProfileReport.BuildBatchedMobilityProfileReport("test", items, clock);
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
        BatchedMobilityProfileReportCountsEachRecordOnce();
    }
}
