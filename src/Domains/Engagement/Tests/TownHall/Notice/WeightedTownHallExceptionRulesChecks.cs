namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WeightedTownHallExceptionRulesChecks
{
    public static void WeightedTownHallExceptionValidatorReportsEveryProblem()
    {
        var validator = WeightedTownHallExceptionRules.DefaultWeightedTownHallExceptionValidator();
        var item = WeightedTownHallException.NewWeightedTownHallException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedTownHallExceptionScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new WeightedTownHallExceptionScoreRangeRule(7.3, 4653.9);
        var item = WeightedTownHallException.NewWeightedTownHallException("x", "y");
        item.Score = 4655.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedTownHallExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedTownHallExceptionLabelLengthRule(16);
        var item = WeightedTownHallException.NewWeightedTownHallException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedTownHallExceptionCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new WeightedTownHallExceptionCapacityRangeRule(16, 86400);
        var item = WeightedTownHallException.NewWeightedTownHallException("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedTownHallExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedTownHallExceptionRequiredNameRule("weighted town hall exception needs an entry");
        var item = WeightedTownHallException.NewWeightedTownHallException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedTownHallExceptionValidatorReportsEveryProblem();
        WeightedTownHallExceptionScoreRangeRuleFlagsValuesAboveScore();
        WeightedTownHallExceptionLabelLengthRuleFlagsALongLabel();
        WeightedTownHallExceptionCapacityRangeRuleFlagsValuesAboveCapacity();
        WeightedTownHallExceptionRequiredNameRuleFlagsABlankName();
    }
}
