namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PendingScenarioAgreementReportChecks
{
    public static void PendingScenarioAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709923080);
        var items = new List<PendingScenarioAgreement>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PendingScenarioAgreement.NewPendingScenarioAgreement($"pending-scenario-agreement-{index}", "sample"));
        }
        var report = PendingScenarioAgreementReport.BuildPendingScenarioAgreementReport("test", items, clock);
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
        PendingScenarioAgreementReportCountsEachRecordOnce();
    }
}
