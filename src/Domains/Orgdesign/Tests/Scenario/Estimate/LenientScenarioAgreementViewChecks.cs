namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LenientScenarioAgreementViewChecks
{
    public static void LenientScenarioAgreementViewShowsTheStatusInCapitals()
    {
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("code-47", "label-400");
        var view = LenientScenarioAgreementView.ToLenientScenarioAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientScenarioAgreementView.ToLenientScenarioAgreementViews(new List<LenientScenarioAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientScenarioAgreementViewShowsTheStatusInCapitals();
    }
}
