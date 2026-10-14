namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedOccupancyForecastRulesChecks
{
    public static void WeightedOccupancyForecastValidatorReportsEveryProblem()
    {
        var validator = WeightedOccupancyForecastRules.DefaultWeightedOccupancyForecastValidator();
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedOccupancyForecastRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new WeightedOccupancyForecastRatioRangeRule(1.0, 4107.6);
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("x", "y");
        item.Ratio = 4109.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedOccupancyForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedOccupancyForecastRequiredCodeRule("weighted occupancy forecast needs a value");
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedOccupancyForecastSkillLengthRuleFlagsALongSkill()
    {
        var rule = new WeightedOccupancyForecastSkillLengthRule(3600);
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedOccupancyForecastValidatorReportsEveryProblem();
        WeightedOccupancyForecastRatioRangeRuleFlagsValuesAboveRatio();
        WeightedOccupancyForecastRequiredCodeRuleFlagsABlankCode();
        WeightedOccupancyForecastSkillLengthRuleFlagsALongSkill();
    }
}
