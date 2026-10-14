namespace Acme.Workforce.Orgdesign.Tests.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

/// <summary>Checks of weighted scenario template code, run by the project's runner.</summary>
public static class PrimaryScenarioAgreementReportChecks
{
    public static void PrimaryScenarioAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709790145);
        var items = new List<PrimaryScenarioAgreement>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryScenarioAgreement.NewPrimaryScenarioAgreement($"primary-scenario-agreement-{index}", "sample"));
        }
        var report = PrimaryScenarioAgreementReport.BuildPrimaryScenarioAgreementReport("test", items, clock);
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
        PrimaryScenarioAgreementReportCountsEachRecordOnce();
    }
}
