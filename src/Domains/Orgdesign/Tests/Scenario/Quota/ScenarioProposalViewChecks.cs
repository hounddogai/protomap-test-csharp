namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioProposalViewChecks
{
    public static void ScenarioProposalViewShowsTheStatusInCapitals()
    {
        var item = ScenarioProposal.NewScenarioProposal("code-852", "title-363");
        var view = ScenarioProposalView.ToScenarioProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioProposalView.ToScenarioProposalViews(new List<ScenarioProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioProposalViewShowsTheStatusInCapitals();
    }
}
