namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioWindowViewChecks
{
    public static void ManualScenarioWindowViewShowsTheStatusInCapitals()
    {
        var item = ManualScenarioWindow.NewManualScenarioWindow("code-613", "title-689");
        var view = ManualScenarioWindowView.ToManualScenarioWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualScenarioWindowView.ToManualScenarioWindowViews(new List<ManualScenarioWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualScenarioWindowViewShowsTheStatusInCapitals();
    }
}
