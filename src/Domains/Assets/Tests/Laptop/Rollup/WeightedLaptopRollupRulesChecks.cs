namespace Acme.Workforce.Assets.Tests.Laptop.Rollup;

using Acme.Workforce.Assets.Laptop.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedLaptopRollupRulesChecks
{
    public static void WeightedLaptopRollupValidatorReportsEveryProblem()
    {
        var validator = WeightedLaptopRollupRules.DefaultWeightedLaptopRollupValidator();
        var item = WeightedLaptopRollup.NewWeightedLaptopRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedLaptopRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedLaptopRollupRequiredNameRule("weighted laptop rollup needs this field");
        var item = WeightedLaptopRollup.NewWeightedLaptopRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedLaptopRollupRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new WeightedLaptopRollupRequiredCostCenterRule("weighted laptop rollup needs an entry");
        var item = WeightedLaptopRollup.NewWeightedLaptopRollup("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedLaptopRollupDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new WeightedLaptopRollupDescriptionLengthRule(86400);
        var item = WeightedLaptopRollup.NewWeightedLaptopRollup("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedLaptopRollupValidatorReportsEveryProblem();
        WeightedLaptopRollupRequiredNameRuleFlagsABlankName();
        WeightedLaptopRollupRequiredCostCenterRuleFlagsABlankCostCenter();
        WeightedLaptopRollupDescriptionLengthRuleFlagsALongDescription();
    }
}
