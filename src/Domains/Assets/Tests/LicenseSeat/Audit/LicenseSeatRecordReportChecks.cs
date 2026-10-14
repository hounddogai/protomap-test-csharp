namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatRecordReportChecks
{
    public static void LicenseSeatRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706109858);
        var items = new List<LicenseSeatRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LicenseSeatRecord.NewLicenseSeatRecord($"license-seat-record-{index}", "sample"));
        }
        var report = LicenseSeatRecordReport.BuildLicenseSeatRecordReport("test", items, clock);
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
        LicenseSeatRecordReportCountsEachRecordOnce();
    }
}
