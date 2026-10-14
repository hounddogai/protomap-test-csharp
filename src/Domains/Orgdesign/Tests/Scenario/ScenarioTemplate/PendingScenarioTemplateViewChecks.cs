namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class PendingScenarioTemplateViewChecks
{
    public static void PendingScenarioTemplateViewShowsTheStatusInCapitals()
    {
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("code-847", "name-335");
        var view = PendingScenarioTemplateView.ToPendingScenarioTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingScenarioTemplateView.ToPendingScenarioTemplateViews(new List<PendingScenarioTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingScenarioTemplateViewShowsTheStatusInCapitals();
    }
}
