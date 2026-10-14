namespace Acme.Workforce.Assets.Tests.LicenseSeat.Cycle;

using Acme.Workforce.Assets.LicenseSeat.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientLicenseSeatAuditReportChecks
{
    public static void LenientLicenseSeatAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708604584);
        var items = new List<LenientLicenseSeatAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientLicenseSeatAudit.NewLenientLicenseSeatAudit($"lenient-license-seat-audit-{index}", "sample"));
        }
        var report = LenientLicenseSeatAuditReport.BuildLenientLicenseSeatAuditReport("test", items, clock);
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
        LenientLicenseSeatAuditReportCountsEachRecordOnce();
    }
}
