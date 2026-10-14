namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientAuditPlanReportChecks
{
    public static void LenientAuditPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708586863);
        var items = new List<LenientAuditPlan>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientAuditPlan.NewLenientAuditPlan($"lenient-audit-plan-{index}", "sample"));
        }
        var report = LenientAuditPlanReport.BuildLenientAuditPlanReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientAuditPlanReportCountsEachRecordOnce();
    }
}
