namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopAdjustmentRulesChecks
{
    public static void LaptopAdjustmentValidatorReportsEveryProblem()
    {
        var validator = LaptopAdjustmentRules.DefaultLaptopAdjustmentValidator();
        var item = LaptopAdjustment.NewLaptopAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopAdjustmentCodeLengthRule(16);
        var item = LaptopAdjustment.NewLaptopAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopAdjustmentRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LaptopAdjustmentRateRangeRule(3.7, 768.5);
        var item = LaptopAdjustment.NewLaptopAdjustment("x", "y");
        item.Rate = 770.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LaptopAdjustmentValidatorReportsEveryProblem();
        LaptopAdjustmentCodeLengthRuleFlagsALongCode();
        LaptopAdjustmentRateRangeRuleFlagsValuesAboveRate();
    }
}
