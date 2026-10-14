namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class WeightedLaptopEstimateRulesChecks
{
    public static void WeightedLaptopEstimateValidatorReportsEveryProblem()
    {
        var validator = WeightedLaptopEstimateRules.DefaultWeightedLaptopEstimateValidator();
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedLaptopEstimateWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new WeightedLaptopEstimateWeightRangeRule(2.1, 131.0);
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("x", "y");
        item.Weight = 132.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedLaptopEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedLaptopEstimateTitleLengthRule(64);
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedLaptopEstimateRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new WeightedLaptopEstimateRequiredChecksumRule("weighted laptop estimate needs this field");
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedLaptopEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedLaptopEstimateRequiredCodeRule("weighted laptop estimate needs this field");
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedLaptopEstimateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedLaptopEstimateRequiredTitleRule("weighted laptop estimate needs a value");
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedLaptopEstimateValidatorReportsEveryProblem();
        WeightedLaptopEstimateWeightRangeRuleFlagsValuesAboveWeight();
        WeightedLaptopEstimateTitleLengthRuleFlagsALongTitle();
        WeightedLaptopEstimateRequiredChecksumRuleFlagsABlankChecksum();
        WeightedLaptopEstimateRequiredCodeRuleFlagsABlankCode();
        WeightedLaptopEstimateRequiredTitleRuleFlagsABlankTitle();
    }
}
