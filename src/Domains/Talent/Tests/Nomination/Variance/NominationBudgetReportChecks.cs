namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class NominationBudgetReportChecks
{
    public static void NominationBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708360936);
        var items = new List<NominationBudget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(NominationBudget.NewNominationBudget($"nomination-budget-{index}", "sample"));
        }
        var report = NominationBudgetReport.BuildNominationBudgetReport("test", items, clock);
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
        NominationBudgetReportCountsEachRecordOnce();
    }
}
