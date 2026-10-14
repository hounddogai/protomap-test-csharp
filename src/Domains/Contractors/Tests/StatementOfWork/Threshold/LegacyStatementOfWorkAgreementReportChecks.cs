namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkAgreementReportChecks
{
    public static void LegacyStatementOfWorkAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706327055);
        var items = new List<LegacyStatementOfWorkAgreement>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement($"legacy-statement-of-work-agreement-{index}", "sample"));
        }
        var report = LegacyStatementOfWorkAgreementReport.BuildLegacyStatementOfWorkAgreementReport("test", items, clock);
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
        LegacyStatementOfWorkAgreementReportCountsEachRecordOnce();
    }
}
