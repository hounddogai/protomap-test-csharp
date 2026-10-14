namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScenarioSlotChecks
{
    public static void ScenarioSlotStartsInOpen()
    {
        var item = ScenarioSlot.NewScenarioSlot("code-853", "name-763");
        Check.Equal(item.Status, ScenarioSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioSlotMovesToArchived()
    {
        var item = ScenarioSlot.NewScenarioSlot("code-853", "name-763");
        try
        {
            item.MoveTo(ScenarioSlotStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioSlotStatus.Archived, "status");
    }

    public static void ScenarioSlotRejectsMovesFromArchived()
    {
        var item = ScenarioSlot.NewScenarioSlot("code-853", "name-763");
        item.Status = ScenarioSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(ScenarioSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScenarioSlotStartsInOpen();
        ScenarioSlotMovesToArchived();
        ScenarioSlotRejectsMovesFromArchived();
    }
}
