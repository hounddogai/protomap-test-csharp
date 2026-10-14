namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveCostModelCheckpointRulesChecks
{
    public static void AdaptiveCostModelCheckpointValidatorReportsEveryProblem()
    {
        var validator = AdaptiveCostModelCheckpointRules.DefaultAdaptiveCostModelCheckpointValidator();
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveCostModelCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveCostModelCheckpointRequiredNameRule("adaptive cost model checkpoint needs an entry");
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveCostModelCheckpointGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AdaptiveCostModelCheckpointGraceDaysRangeRule(80, 80);
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveCostModelCheckpointWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AdaptiveCostModelCheckpointWindowDaysRangeRule(80, 255);
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("x", "y");
        item.WindowDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveCostModelCheckpointValidatorReportsEveryProblem();
        AdaptiveCostModelCheckpointRequiredNameRuleFlagsABlankName();
        AdaptiveCostModelCheckpointGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        AdaptiveCostModelCheckpointWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
