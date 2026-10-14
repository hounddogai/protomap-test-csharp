namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LicenseSeatAllocationReportChecks
{
    public static void LicenseSeatAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707744518);
        var items = new List<LicenseSeatAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LicenseSeatAllocation.NewLicenseSeatAllocation($"license-seat-allocation-{index}", "sample"));
        }
        var report = LicenseSeatAllocationReport.BuildLicenseSeatAllocationReport("test", items, clock);
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
        LicenseSeatAllocationReportCountsEachRecordOnce();
    }
}
