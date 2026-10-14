namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PendingScenarioAgreementViewChecks
{
    public static void PendingScenarioAgreementViewShowsTheStatusInCapitals()
    {
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("code-54", "label-9");
        var view = PendingScenarioAgreementView.ToPendingScenarioAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingScenarioAgreementView.ToPendingScenarioAgreementViews(new List<PendingScenarioAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingScenarioAgreementViewShowsTheStatusInCapitals();
    }
}
