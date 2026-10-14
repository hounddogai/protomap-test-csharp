namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class PayslipPreferenceReportChecks
{
    public static void PayslipPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703189628);
        var items = new List<PayslipPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PayslipPreference.NewPayslipPreference($"payslip-preference-{index}", "sample"));
        }
        var report = PayslipPreferenceReport.BuildPayslipPreferenceReport("test", items, clock);
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
        PayslipPreferenceReportCountsEachRecordOnce();
    }
}
