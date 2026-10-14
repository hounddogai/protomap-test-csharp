namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatProfileReportChecks
{
    public static void LicenseSeatProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706240222);
        var items = new List<LicenseSeatProfile>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LicenseSeatProfile.NewLicenseSeatProfile($"license-seat-profile-{index}", "sample"));
        }
        var report = LicenseSeatProfileReport.BuildLicenseSeatProfileReport("test", items, clock);
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
        LicenseSeatProfileReportCountsEachRecordOnce();
    }
}
