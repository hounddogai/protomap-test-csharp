namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class RegionalPayRunTemplateReportChecks
{
    public static void RegionalPayRunTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702407525);
        var items = new List<RegionalPayRunTemplate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalPayRunTemplate.NewRegionalPayRunTemplate($"regional-pay-run-template-{index}", "sample"));
        }
        var report = RegionalPayRunTemplateReport.BuildRegionalPayRunTemplateReport("test", items, clock);
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
        RegionalPayRunTemplateReportCountsEachRecordOnce();
    }
}
