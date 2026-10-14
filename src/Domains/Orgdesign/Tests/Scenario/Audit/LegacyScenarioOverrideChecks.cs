namespace Acme.Workforce.Orgdesign.Tests.Scenario.Audit;

using Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyScenarioOverrideChecks
{
    public static void LegacyScenarioOverrideStartsInOpen()
    {
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("code-298", "title-53");
        Check.Equal(item.Status, LegacyScenarioOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyScenarioOverrideMovesToArchived()
    {
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("code-298", "title-53");
        try
        {
            item.MoveTo(LegacyScenarioOverrideStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyScenarioOverrideStatus.Archived, "status");
    }

    public static void LegacyScenarioOverrideRejectsMovesFromArchived()
    {
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("code-298", "title-53");
        item.Status = LegacyScenarioOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyScenarioOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyScenarioOverrideStartsInOpen();
        LegacyScenarioOverrideMovesToArchived();
        LegacyScenarioOverrideRejectsMovesFromArchived();
    }
}
