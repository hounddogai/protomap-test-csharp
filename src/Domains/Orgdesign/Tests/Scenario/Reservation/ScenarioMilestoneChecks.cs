namespace Acme.Workforce.Orgdesign.Tests.Scenario.Reservation;

using Acme.Workforce.Orgdesign.Scenario.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScenarioMilestoneChecks
{
    public static void ScenarioMilestoneStartsInIdle()
    {
        var item = ScenarioMilestone.NewScenarioMilestone("code-309", "title-880");
        Check.Equal(item.Status, ScenarioMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioMilestoneMovesToReady()
    {
        var item = ScenarioMilestone.NewScenarioMilestone("code-309", "title-880");
        try
        {
            item.MoveTo(ScenarioMilestoneStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioMilestoneStatus.Ready, "status");
    }

    public static void ScenarioMilestoneRejectsMovesFromStopped()
    {
        var item = ScenarioMilestone.NewScenarioMilestone("code-309", "title-880");
        item.Status = ScenarioMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScenarioMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScenarioMilestoneStartsInIdle();
        ScenarioMilestoneMovesToReady();
        ScenarioMilestoneRejectsMovesFromStopped();
    }
}
