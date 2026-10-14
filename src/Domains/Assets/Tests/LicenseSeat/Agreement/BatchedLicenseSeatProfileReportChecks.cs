namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class BatchedLicenseSeatProfileReportChecks
{
    public static void BatchedLicenseSeatProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707888265);
        var items = new List<BatchedLicenseSeatProfile>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile($"batched-license-seat-profile-{index}", "sample"));
        }
        var report = BatchedLicenseSeatProfileReport.BuildBatchedLicenseSeatProfileReport("test", items, clock);
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
        BatchedLicenseSeatProfileReportCountsEachRecordOnce();
    }
}
