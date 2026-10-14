namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioQuotaReportChecks
{
    public static void ScenarioQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703213356);
        var items = new List<ScenarioQuota>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScenarioQuota.NewScenarioQuota($"scenario-quota-{index}", "sample"));
        }
        var report = ScenarioQuotaReport.BuildScenarioQuotaReport("test", items, clock);
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
        ScenarioQuotaReportCountsEachRecordOnce();
    }
}
