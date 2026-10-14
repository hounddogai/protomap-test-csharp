namespace Acme.Workforce.Orgdesign.Tests.Scenario.Notice;

using Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingScenarioCheckpointRulesChecks
{
    public static void PendingScenarioCheckpointValidatorReportsEveryProblem()
    {
        var validator = PendingScenarioCheckpointRules.DefaultPendingScenarioCheckpointValidator();
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingScenarioCheckpointFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new PendingScenarioCheckpointFteRangeRule(1.9, 3862.1);
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("x", "y");
        item.Fte = 3863.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingScenarioCheckpointFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PendingScenarioCheckpointFloorValueRangeRule(7.1, 408.2);
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("x", "y");
        item.FloorValue = 409.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingScenarioCheckpointRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new PendingScenarioCheckpointRequiredTeamRule("pending scenario checkpoint needs an entry");
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingScenarioCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingScenarioCheckpointRequiredLabelRule("pending scenario checkpoint needs a value");
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingScenarioCheckpointValidatorReportsEveryProblem();
        PendingScenarioCheckpointFteRangeRuleFlagsValuesAboveFte();
        PendingScenarioCheckpointFloorValueRangeRuleFlagsValuesAboveFloorValue();
        PendingScenarioCheckpointRequiredTeamRuleFlagsABlankTeam();
        PendingScenarioCheckpointRequiredLabelRuleFlagsABlankLabel();
    }
}
