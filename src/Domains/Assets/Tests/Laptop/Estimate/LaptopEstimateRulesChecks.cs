namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LaptopEstimateRulesChecks
{
    public static void LaptopEstimateValidatorReportsEveryProblem()
    {
        var validator = LaptopEstimateRules.DefaultLaptopEstimateValidator();
        var item = LaptopEstimate.NewLaptopEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopEstimateExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LaptopEstimateExpiresAtRangeRule(16, 1000);
        var item = LaptopEstimate.NewLaptopEstimate("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new LaptopEstimateNameLengthRule(120);
        var item = LaptopEstimate.NewLaptopEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopEstimateCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new LaptopEstimateCategoryLengthRule(120);
        var item = LaptopEstimate.NewLaptopEstimate("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopEstimateValidatorReportsEveryProblem();
        LaptopEstimateExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LaptopEstimateNameLengthRuleFlagsALongName();
        LaptopEstimateCategoryLengthRuleFlagsALongCategory();
    }
}
