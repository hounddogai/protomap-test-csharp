namespace Acme.Workforce.Payrollops.Tests.RetroPay.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RetroPayPolicyReportChecks
{
    public static void RetroPayPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709711791);
        var items = new List<RetroPayPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RetroPayPolicy.NewRetroPayPolicy($"retro-pay-policy-{index}", "sample"));
        }
        var report = RetroPayPolicyReport.BuildRetroPayPolicyReport("test", items, clock);
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
        RetroPayPolicyReportCountsEachRecordOnce();
    }
}
