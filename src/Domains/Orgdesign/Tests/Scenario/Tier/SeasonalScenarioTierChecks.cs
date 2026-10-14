namespace Acme.Workforce.Orgdesign.Tests.Scenario.Tier;

using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class SeasonalScenarioTierChecks
{
    public static void SeasonalScenarioTierStartsInCreated()
    {
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("code-502", "name-356");
        Check.Equal(item.Status, SeasonalScenarioTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalScenarioTierMovesToPublished()
    {
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("code-502", "name-356");
        try
        {
            item.MoveTo(SeasonalScenarioTierStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalScenarioTierStatus.Published, "status");
    }

    public static void SeasonalScenarioTierRejectsMovesFromRetired()
    {
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("code-502", "name-356");
        item.Status = SeasonalScenarioTierStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalScenarioTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalScenarioTierStartsInCreated();
        SeasonalScenarioTierMovesToPublished();
        SeasonalScenarioTierRejectsMovesFromRetired();
    }
}
