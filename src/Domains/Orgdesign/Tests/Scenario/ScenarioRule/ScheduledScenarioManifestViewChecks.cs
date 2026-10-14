namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class ScheduledScenarioManifestViewChecks
{
    public static void ScheduledScenarioManifestViewShowsTheStatusInCapitals()
    {
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("code-75", "name-699");
        var view = ScheduledScenarioManifestView.ToScheduledScenarioManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledScenarioManifestView.ToScheduledScenarioManifestViews(new List<ScheduledScenarioManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledScenarioManifestViewShowsTheStatusInCapitals();
    }
}
