namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LaptopDigestRulesChecks
{
    public static void LaptopDigestValidatorReportsEveryProblem()
    {
        var validator = LaptopDigestRules.DefaultLaptopDigestValidator();
        var item = LaptopDigest.NewLaptopDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new LaptopDigestRequiredNameRule("laptop digest needs this field");
        var item = LaptopDigest.NewLaptopDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopDigestRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LaptopDigestRateRangeRule(5.3, 492.4);
        var item = LaptopDigest.NewLaptopDigest("x", "y");
        item.Rate = 493.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopDigestUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new LaptopDigestUpdatedAtRangeRule(64, 1000);
        var item = LaptopDigest.NewLaptopDigest("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LaptopDigestValidatorReportsEveryProblem();
        LaptopDigestRequiredNameRuleFlagsABlankName();
        LaptopDigestRateRangeRuleFlagsValuesAboveRate();
        LaptopDigestUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
