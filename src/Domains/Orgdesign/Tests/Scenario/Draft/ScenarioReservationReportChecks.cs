namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScenarioReservationReportChecks
{
    public static void ScenarioReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705077677);
        var items = new List<ScenarioReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScenarioReservation.NewScenarioReservation($"scenario-reservation-{index}", "sample"));
        }
        var report = ScenarioReservationReport.BuildScenarioReservationReport("test", items, clock);
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
        ScenarioReservationReportCountsEachRecordOnce();
    }
}
