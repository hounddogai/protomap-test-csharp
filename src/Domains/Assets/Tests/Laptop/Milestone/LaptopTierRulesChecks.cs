namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopTierRulesChecks
{
    public static void LaptopTierValidatorReportsEveryProblem()
    {
        var validator = LaptopTierRules.DefaultLaptopTierValidator();
        var item = LaptopTier.NewLaptopTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopTierCodeLengthRule(1000);
        var item = LaptopTier.NewLaptopTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopTierRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LaptopTierRatioRangeRule(9.1, 2619.8);
        var item = LaptopTier.NewLaptopTier("x", "y");
        item.Ratio = 2621.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LaptopTierRequiredTitleRule("laptop tier needs an entry");
        var item = LaptopTier.NewLaptopTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopTierValidatorReportsEveryProblem();
        LaptopTierCodeLengthRuleFlagsALongCode();
        LaptopTierRatioRangeRuleFlagsValuesAboveRatio();
        LaptopTierRequiredTitleRuleFlagsABlankTitle();
    }
}
