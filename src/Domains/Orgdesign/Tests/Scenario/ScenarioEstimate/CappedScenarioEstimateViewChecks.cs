namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class CappedScenarioEstimateViewChecks
{
    public static void CappedScenarioEstimateViewShowsTheStatusInCapitals()
    {
        var item = CappedScenarioEstimate.NewCappedScenarioEstimate("code-782", "title-369");
        var view = CappedScenarioEstimateView.ToCappedScenarioEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedScenarioEstimateView.ToCappedScenarioEstimateViews(new List<CappedScenarioEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedScenarioEstimateViewShowsTheStatusInCapitals();
    }
}
