namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedBalanceRequestReportChecks
{
    public static void CappedBalanceRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701382535);
        var items = new List<CappedBalanceRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedBalanceRequest.NewCappedBalanceRequest($"capped-balance-request-{index}", "sample"));
        }
        var report = CappedBalanceRequestReport.BuildCappedBalanceRequestReport("test", items, clock);
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
        CappedBalanceRequestReportCountsEachRecordOnce();
    }
}
