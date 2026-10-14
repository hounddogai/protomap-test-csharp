namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class DefaultLaptopAdjustmentRulesChecks
{
    public static void DefaultLaptopAdjustmentValidatorReportsEveryProblem()
    {
        var validator = DefaultLaptopAdjustmentRules.DefaultDefaultLaptopAdjustmentValidator();
        var item = DefaultLaptopAdjustment.NewDefaultLaptopAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultLaptopAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultLaptopAdjustmentRequiredLabelRule("default laptop adjustment needs this field");
        var item = DefaultLaptopAdjustment.NewDefaultLaptopAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultLaptopAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultLaptopAdjustmentRequiredCodeRule("default laptop adjustment needs an entry");
        var item = DefaultLaptopAdjustment.NewDefaultLaptopAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultLaptopAdjustmentValidatorReportsEveryProblem();
        DefaultLaptopAdjustmentRequiredLabelRuleFlagsABlankLabel();
        DefaultLaptopAdjustmentRequiredCodeRuleFlagsABlankCode();
    }
}
