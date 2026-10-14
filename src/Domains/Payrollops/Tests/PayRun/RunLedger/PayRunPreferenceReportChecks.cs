namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class PayRunPreferenceReportChecks
{
    public static void PayRunPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702338249);
        var items = new List<PayRunPreference>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PayRunPreference.NewPayRunPreference($"pay-run-preference-{index}", "sample"));
        }
        var report = PayRunPreferenceReport.BuildPayRunPreferenceReport("test", items, clock);
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
        PayRunPreferenceReportCountsEachRecordOnce();
    }
}
