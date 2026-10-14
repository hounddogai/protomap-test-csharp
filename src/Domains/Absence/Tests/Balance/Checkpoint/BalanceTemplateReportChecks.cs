namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BalanceTemplateReportChecks
{
    public static void BalanceTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706414427);
        var items = new List<BalanceTemplate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BalanceTemplate.NewBalanceTemplate($"balance-template-{index}", "sample"));
        }
        var report = BalanceTemplateReport.BuildBalanceTemplateReport("test", items, clock);
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
        BalanceTemplateReportCountsEachRecordOnce();
    }
}
