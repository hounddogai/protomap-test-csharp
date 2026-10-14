namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingScenarioRequestViewChecks
{
    public static void PendingScenarioRequestViewShowsTheStatusInCapitals()
    {
        var item = PendingScenarioRequest.NewPendingScenarioRequest("code-596", "label-705");
        var view = PendingScenarioRequestView.ToPendingScenarioRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingScenarioRequestView.ToPendingScenarioRequestViews(new List<PendingScenarioRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingScenarioRequestViewShowsTheStatusInCapitals();
    }
}
