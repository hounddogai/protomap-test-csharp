namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PendingRetroPayRuleReportChecks
{
    public static void PendingRetroPayRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704161398);
        var items = new List<PendingRetroPayRule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PendingRetroPayRule.NewPendingRetroPayRule($"pending-retro-pay-rule-{index}", "sample"));
        }
        var report = PendingRetroPayRuleReport.BuildPendingRetroPayRuleReport("test", items, clock);
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
        PendingRetroPayRuleReportCountsEachRecordOnce();
    }
}
