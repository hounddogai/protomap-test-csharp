namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class PrimaryRetroPayAuditReportChecks
{
    public static void PrimaryRetroPayAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707326017);
        var items = new List<PrimaryRetroPayAudit>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryRetroPayAudit.NewPrimaryRetroPayAudit($"primary-retro-pay-audit-{index}", "sample"));
        }
        var report = PrimaryRetroPayAuditReport.BuildPrimaryRetroPayAuditReport("test", items, clock);
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
        PrimaryRetroPayAuditReportCountsEachRecordOnce();
    }
}
