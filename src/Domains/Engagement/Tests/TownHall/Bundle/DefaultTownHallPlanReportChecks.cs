namespace Acme.Workforce.Engagement.Tests.TownHall.Bundle;

using Acme.Workforce.Engagement.TownHall.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class DefaultTownHallPlanReportChecks
{
    public static void DefaultTownHallPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702692608);
        var items = new List<DefaultTownHallPlan>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultTownHallPlan.NewDefaultTownHallPlan($"default-town-hall-plan-{index}", "sample"));
        }
        var report = DefaultTownHallPlanReport.BuildDefaultTownHallPlanReport("test", items, clock);
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
        DefaultTownHallPlanReportCountsEachRecordOnce();
    }
}
