namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ManualScenarioEstimateViewChecks
{
    public static void ManualScenarioEstimateViewShowsTheStatusInCapitals()
    {
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("code-750", "name-890");
        var view = ManualScenarioEstimateView.ToManualScenarioEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualScenarioEstimateView.ToManualScenarioEstimateViews(new List<ManualScenarioEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualScenarioEstimateViewShowsTheStatusInCapitals();
    }
}
