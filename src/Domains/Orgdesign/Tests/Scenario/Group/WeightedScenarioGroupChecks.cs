namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedScenarioGroupChecks
{
    public static void WeightedScenarioGroupStartsInNew()
    {
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("code-267", "title-172");
        Check.Equal(item.Status, WeightedScenarioGroupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedScenarioGroupMovesToFailed()
    {
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("code-267", "title-172");
        try
        {
            item.MoveTo(WeightedScenarioGroupStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedScenarioGroupStatus.Failed, "status");
    }

    public static void WeightedScenarioGroupRejectsMovesFromFailed()
    {
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("code-267", "title-172");
        item.Status = WeightedScenarioGroupStatus.Failed;
        Check.Fails(() => item.MoveTo(WeightedScenarioGroupStatus.New), "move");
    }

    public static void RunAll()
    {
        WeightedScenarioGroupStartsInNew();
        WeightedScenarioGroupMovesToFailed();
        WeightedScenarioGroupRejectsMovesFromFailed();
    }
}
