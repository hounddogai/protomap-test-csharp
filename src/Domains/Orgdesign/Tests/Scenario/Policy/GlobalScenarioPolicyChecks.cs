namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalScenarioPolicyChecks
{
    public static void GlobalScenarioPolicyStartsInCreated()
    {
        var item = GlobalScenarioPolicy.NewGlobalScenarioPolicy("code-231", "name-659");
        Check.Equal(item.Status, GlobalScenarioPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalScenarioPolicyMovesToRetired()
    {
        var item = GlobalScenarioPolicy.NewGlobalScenarioPolicy("code-231", "name-659");
        try
        {
            item.MoveTo(GlobalScenarioPolicyStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalScenarioPolicyStatus.Retired, "status");
    }

    public static void GlobalScenarioPolicyRejectsMovesFromRetired()
    {
        var item = GlobalScenarioPolicy.NewGlobalScenarioPolicy("code-231", "name-659");
        item.Status = GlobalScenarioPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalScenarioPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalScenarioPolicyStartsInCreated();
        GlobalScenarioPolicyMovesToRetired();
        GlobalScenarioPolicyRejectsMovesFromRetired();
    }
}
