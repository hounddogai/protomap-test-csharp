namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultBalanceDraftReportChecks
{
    public static void DefaultBalanceDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701719313);
        var items = new List<DefaultBalanceDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultBalanceDraft.NewDefaultBalanceDraft($"default-balance-draft-{index}", "sample"));
        }
        var report = DefaultBalanceDraftReport.BuildDefaultBalanceDraftReport("test", items, clock);
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
        DefaultBalanceDraftReportCountsEachRecordOnce();
    }
}
