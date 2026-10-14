namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioRuleChecks
{
    public static void ManualScenarioRuleStartsInNew()
    {
        var item = ManualScenarioRule.NewManualScenarioRule("code-542", "label-479");
        Check.Equal(item.Status, ManualScenarioRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualScenarioRuleMovesToRunning()
    {
        var item = ManualScenarioRule.NewManualScenarioRule("code-542", "label-479");
        try
        {
            item.MoveTo(ManualScenarioRuleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualScenarioRuleStatus.Running, "status");
    }

    public static void ManualScenarioRuleRejectsMovesFromFailed()
    {
        var item = ManualScenarioRule.NewManualScenarioRule("code-542", "label-479");
        item.Status = ManualScenarioRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualScenarioRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualScenarioRuleStartsInNew();
        ManualScenarioRuleMovesToRunning();
        ManualScenarioRuleRejectsMovesFromFailed();
    }
}
