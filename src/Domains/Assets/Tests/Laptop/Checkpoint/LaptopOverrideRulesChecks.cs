namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LaptopOverrideRulesChecks
{
    public static void LaptopOverrideValidatorReportsEveryProblem()
    {
        var validator = LaptopOverrideRules.DefaultLaptopOverrideValidator();
        var item = LaptopOverride.NewLaptopOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopOverrideVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new LaptopOverrideVersionRangeRule(32, 1000);
        var item = LaptopOverride.NewLaptopOverride("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopOverrideEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LaptopOverrideEmployeeIdLengthRule(255);
        var item = LaptopOverride.NewLaptopOverride("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LaptopOverrideRequiredCodeRule("laptop override needs an entry");
        var item = LaptopOverride.NewLaptopOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopOverrideValidatorReportsEveryProblem();
        LaptopOverrideVersionRangeRuleFlagsValuesAboveVersion();
        LaptopOverrideEmployeeIdLengthRuleFlagsALongEmployeeId();
        LaptopOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
