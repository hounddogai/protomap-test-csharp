namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class TieredScenarioPolicyChecks
{
    public static void TieredScenarioPolicyStartsInNew()
    {
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("code-485", "title-297");
        Check.Equal(item.Status, TieredScenarioPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredScenarioPolicyMovesToQueued()
    {
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("code-485", "title-297");
        try
        {
            item.MoveTo(TieredScenarioPolicyStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredScenarioPolicyStatus.Queued, "status");
    }

    public static void TieredScenarioPolicyRejectsMovesFromFailed()
    {
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("code-485", "title-297");
        item.Status = TieredScenarioPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(TieredScenarioPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        TieredScenarioPolicyStartsInNew();
        TieredScenarioPolicyMovesToQueued();
        TieredScenarioPolicyRejectsMovesFromFailed();
    }
}
