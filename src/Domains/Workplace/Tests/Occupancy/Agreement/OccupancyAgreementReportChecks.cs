namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OccupancyAgreementReportChecks
{
    public static void OccupancyAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703865972);
        var items = new List<OccupancyAgreement>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(OccupancyAgreement.NewOccupancyAgreement($"occupancy-agreement-{index}", "sample"));
        }
        var report = OccupancyAgreementReport.BuildOccupancyAgreementReport("test", items, clock);
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
        OccupancyAgreementReportCountsEachRecordOnce();
    }
}
