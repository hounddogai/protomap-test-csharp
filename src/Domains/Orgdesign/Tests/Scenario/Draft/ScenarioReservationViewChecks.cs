namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScenarioReservationViewChecks
{
    public static void ScenarioReservationViewShowsTheStatusInCapitals()
    {
        var item = ScenarioReservation.NewScenarioReservation("code-546", "title-817");
        var view = ScenarioReservationView.ToScenarioReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioReservationView.ToScenarioReservationViews(new List<ScenarioReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioReservationViewShowsTheStatusInCapitals();
    }
}
