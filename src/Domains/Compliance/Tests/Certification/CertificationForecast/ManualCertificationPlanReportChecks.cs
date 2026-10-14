namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationPlanReportChecks
{
    public static void ManualCertificationPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707495703);
        var items = new List<ManualCertificationPlan>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualCertificationPlan.NewManualCertificationPlan($"manual-certification-plan-{index}", "sample"));
        }
        var report = ManualCertificationPlanReport.BuildManualCertificationPlanReport("test", items, clock);
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
        ManualCertificationPlanReportCountsEachRecordOnce();
    }
}
