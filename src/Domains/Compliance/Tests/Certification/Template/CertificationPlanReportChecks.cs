namespace Acme.Workforce.Compliance.Tests.Certification.Template;

using Acme.Workforce.Compliance.Certification.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CertificationPlanReportChecks
{
    public static void CertificationPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705934753);
        var items = new List<CertificationPlan>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CertificationPlan.NewCertificationPlan($"certification-plan-{index}", "sample"));
        }
        var report = CertificationPlanReport.BuildCertificationPlanReport("test", items, clock);
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
        CertificationPlanReportCountsEachRecordOnce();
    }
}
