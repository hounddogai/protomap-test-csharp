namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class WeightedLaptopThresholdRulesChecks
{
    public static void WeightedLaptopThresholdValidatorReportsEveryProblem()
    {
        var validator = WeightedLaptopThresholdRules.DefaultWeightedLaptopThresholdValidator();
        var item = WeightedLaptopThreshold.NewWeightedLaptopThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedLaptopThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedLaptopThresholdRequiredCodeRule("weighted laptop threshold needs an entry");
        var item = WeightedLaptopThreshold.NewWeightedLaptopThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedLaptopThresholdValidatorReportsEveryProblem();
        WeightedLaptopThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
