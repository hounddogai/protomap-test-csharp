namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientStatementOfWorkBundleReportChecks
{
    public static void LenientStatementOfWorkBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702468230);
        var items = new List<LenientStatementOfWorkBundle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle($"lenient-statement-of-work-bundle-{index}", "sample"));
        }
        var report = LenientStatementOfWorkBundleReport.BuildLenientStatementOfWorkBundleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkBundleReportCountsEachRecordOnce();
    }
}
