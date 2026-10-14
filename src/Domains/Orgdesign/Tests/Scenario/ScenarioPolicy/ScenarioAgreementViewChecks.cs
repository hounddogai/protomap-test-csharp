namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioAgreementViewChecks
{
    public static void ScenarioAgreementViewShowsTheStatusInCapitals()
    {
        var item = ScenarioAgreement.NewScenarioAgreement("code-747", "name-426");
        var view = ScenarioAgreementView.ToScenarioAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioAgreementView.ToScenarioAgreementViews(new List<ScenarioAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioAgreementViewShowsTheStatusInCapitals();
    }
}
