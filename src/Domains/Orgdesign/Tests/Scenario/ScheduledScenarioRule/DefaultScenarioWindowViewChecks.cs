namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class DefaultScenarioWindowViewChecks
{
    public static void DefaultScenarioWindowViewShowsTheStatusInCapitals()
    {
        var item = DefaultScenarioWindow.NewDefaultScenarioWindow("code-439", "label-870");
        var view = DefaultScenarioWindowView.ToDefaultScenarioWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultScenarioWindowView.ToDefaultScenarioWindowViews(new List<DefaultScenarioWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultScenarioWindowViewShowsTheStatusInCapitals();
    }
}
