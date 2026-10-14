namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountAgreementReportChecks
{
    public static void HeadcountAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706937991);
        var items = new List<HeadcountAgreement>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(HeadcountAgreement.NewHeadcountAgreement($"headcount-agreement-{index}", "sample"));
        }
        var report = HeadcountAgreementReport.BuildHeadcountAgreementReport("test", items, clock);
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
        HeadcountAgreementReportCountsEachRecordOnce();
    }
}
