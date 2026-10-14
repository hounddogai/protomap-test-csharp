namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledBalanceAgreementReportChecks
{
    public static void ScheduledBalanceAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705790888);
        var items = new List<ScheduledBalanceAgreement>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledBalanceAgreement.NewScheduledBalanceAgreement($"scheduled-balance-agreement-{index}", "sample"));
        }
        var report = ScheduledBalanceAgreementReport.BuildScheduledBalanceAgreementReport("test", items, clock);
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
        ScheduledBalanceAgreementReportCountsEachRecordOnce();
    }
}
