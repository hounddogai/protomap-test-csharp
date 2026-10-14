namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class PrimaryTownHallRuleReportChecks
{
    public static void PrimaryTownHallRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703586244);
        var items = new List<PrimaryTownHallRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryTownHallRule.NewPrimaryTownHallRule($"primary-town-hall-rule-{index}", "sample"));
        }
        var report = PrimaryTownHallRuleReport.BuildPrimaryTownHallRuleReport("test", items, clock);
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
        PrimaryTownHallRuleReportCountsEachRecordOnce();
    }
}
