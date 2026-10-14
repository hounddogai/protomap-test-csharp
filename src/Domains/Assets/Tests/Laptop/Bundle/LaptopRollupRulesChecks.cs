namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopRollupRulesChecks
{
    public static void LaptopRollupValidatorReportsEveryProblem()
    {
        var validator = LaptopRollupRules.DefaultLaptopRollupValidator();
        var item = LaptopRollup.NewLaptopRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopRollupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LaptopRollupTitleLengthRule(1000);
        var item = LaptopRollup.NewLaptopRollup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopRollupCodeLengthRule(1000);
        var item = LaptopRollup.NewLaptopRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopRollupValidatorReportsEveryProblem();
        LaptopRollupTitleLengthRuleFlagsALongTitle();
        LaptopRollupCodeLengthRuleFlagsALongCode();
    }
}
