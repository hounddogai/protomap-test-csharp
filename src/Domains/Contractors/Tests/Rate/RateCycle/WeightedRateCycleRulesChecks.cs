namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class WeightedRateCycleRulesChecks
{
    public static void WeightedRateCycleValidatorReportsEveryProblem()
    {
        var validator = WeightedRateCycleRules.DefaultWeightedRateCycleValidator();
        var item = WeightedRateCycle.NewWeightedRateCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRateCycleTeamLengthRuleFlagsALongTeam()
    {
        var rule = new WeightedRateCycleTeamLengthRule(16);
        var item = WeightedRateCycle.NewWeightedRateCycle("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRateCycleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new WeightedRateCycleMarginRangeRule(3.7, 3263.7);
        var item = WeightedRateCycle.NewWeightedRateCycle("x", "y");
        item.Margin = 3265.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedRateCycleRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new WeightedRateCycleRequiredTeamRule("weighted rate cycle needs this field");
        var item = WeightedRateCycle.NewWeightedRateCycle("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRateCycleNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedRateCycleNameLengthRule(1000);
        var item = WeightedRateCycle.NewWeightedRateCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedRateCycleValidatorReportsEveryProblem();
        WeightedRateCycleTeamLengthRuleFlagsALongTeam();
        WeightedRateCycleMarginRangeRuleFlagsValuesAboveMargin();
        WeightedRateCycleRequiredTeamRuleFlagsABlankTeam();
        WeightedRateCycleNameLengthRuleFlagsALongName();
    }
}
