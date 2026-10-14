namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class WeightedScenarioOverrideChecks
{
    public static void WeightedScenarioOverrideStartsInOpen()
    {
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("code-562", "title-217");
        Check.Equal(item.Status, WeightedScenarioOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedScenarioOverrideMovesToInReview()
    {
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("code-562", "title-217");
        try
        {
            item.MoveTo(WeightedScenarioOverrideStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedScenarioOverrideStatus.InReview, "status");
    }

    public static void WeightedScenarioOverrideRejectsMovesFromArchived()
    {
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("code-562", "title-217");
        item.Status = WeightedScenarioOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedScenarioOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedScenarioOverrideStartsInOpen();
        WeightedScenarioOverrideMovesToInReview();
        WeightedScenarioOverrideRejectsMovesFromArchived();
    }
}
