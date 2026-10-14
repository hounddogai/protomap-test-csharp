namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class PayBandRuleReportChecks
{
    public static void PayBandRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705630334);
        var items = new List<PayBandRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayBandRule.NewPayBandRule($"pay-band-rule-{index}", "sample"));
        }
        var report = PayBandRuleReport.BuildPayBandRuleReport("test", items, clock);
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
        PayBandRuleReportCountsEachRecordOnce();
    }
}
