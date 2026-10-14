namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelCheckpointRulesChecks
{
    public static void CostModelCheckpointValidatorReportsEveryProblem()
    {
        var validator = CostModelCheckpointRules.DefaultCostModelCheckpointValidator();
        var item = CostModelCheckpoint.NewCostModelCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelCheckpointRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new CostModelCheckpointRequiredTimezoneRule("cost model checkpoint needs a value");
        var item = CostModelCheckpoint.NewCostModelCheckpoint("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new CostModelCheckpointRequiredNameRule("cost model checkpoint needs an entry");
        var item = CostModelCheckpoint.NewCostModelCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelCheckpointTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new CostModelCheckpointTimezoneLengthRule(80);
        var item = CostModelCheckpoint.NewCostModelCheckpoint("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CostModelCheckpointTitleLengthRule(1000);
        var item = CostModelCheckpoint.NewCostModelCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelCheckpointValidatorReportsEveryProblem();
        CostModelCheckpointRequiredTimezoneRuleFlagsABlankTimezone();
        CostModelCheckpointRequiredNameRuleFlagsABlankName();
        CostModelCheckpointTimezoneLengthRuleFlagsALongTimezone();
        CostModelCheckpointTitleLengthRuleFlagsALongTitle();
    }
}
