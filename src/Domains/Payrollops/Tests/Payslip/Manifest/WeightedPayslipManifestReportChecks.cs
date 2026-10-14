namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WeightedPayslipManifestReportChecks
{
    public static void WeightedPayslipManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701684589);
        var items = new List<WeightedPayslipManifest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WeightedPayslipManifest.NewWeightedPayslipManifest($"weighted-payslip-manifest-{index}", "sample"));
        }
        var report = WeightedPayslipManifestReport.BuildWeightedPayslipManifestReport("test", items, clock);
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
        WeightedPayslipManifestReportCountsEachRecordOnce();
    }
}
