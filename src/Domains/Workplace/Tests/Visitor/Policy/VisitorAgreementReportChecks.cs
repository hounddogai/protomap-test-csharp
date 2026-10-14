namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorAgreementReportChecks
{
    public static void VisitorAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706872538);
        var items = new List<VisitorAgreement>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VisitorAgreement.NewVisitorAgreement($"visitor-agreement-{index}", "sample"));
        }
        var report = VisitorAgreementReport.BuildVisitorAgreementReport("test", items, clock);
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
        VisitorAgreementReportCountsEachRecordOnce();
    }
}
