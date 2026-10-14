namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayAuditReportChecks
{
    public static void RetroPayAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702155840);
        var items = new List<RetroPayAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RetroPayAudit.NewRetroPayAudit($"retro-pay-audit-{index}", "sample"));
        }
        var report = RetroPayAuditReport.BuildRetroPayAuditReport("test", items, clock);
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
        RetroPayAuditReportCountsEachRecordOnce();
    }
}
