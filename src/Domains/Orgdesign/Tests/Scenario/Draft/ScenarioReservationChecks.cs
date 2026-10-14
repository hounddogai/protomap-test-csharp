namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScenarioReservationChecks
{
    public static void ScenarioReservationStartsInIdle()
    {
        var item = ScenarioReservation.NewScenarioReservation("code-546", "title-817");
        Check.Equal(item.Status, ScenarioReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioReservationMovesToWarming()
    {
        var item = ScenarioReservation.NewScenarioReservation("code-546", "title-817");
        try
        {
            item.MoveTo(ScenarioReservationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioReservationStatus.Warming, "status");
    }

    public static void ScenarioReservationRejectsMovesFromStopped()
    {
        var item = ScenarioReservation.NewScenarioReservation("code-546", "title-817");
        item.Status = ScenarioReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScenarioReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScenarioReservationStartsInIdle();
        ScenarioReservationMovesToWarming();
        ScenarioReservationRejectsMovesFromStopped();
    }
}
