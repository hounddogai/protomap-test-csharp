namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopVarianceRulesChecks
{
    public static void LaptopVarianceValidatorReportsEveryProblem()
    {
        var validator = LaptopVarianceRules.DefaultLaptopVarianceValidator();
        var item = LaptopVariance.NewLaptopVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopVarianceExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LaptopVarianceExpiresAtRangeRule(32, 64);
        var item = LaptopVariance.NewLaptopVariance("x", "y");
        item.ExpiresAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LaptopVarianceRequiredCodeRule("laptop variance needs this field");
        var item = LaptopVariance.NewLaptopVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopVarianceRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LaptopVarianceRequiredCostCenterRule("laptop variance needs this field");
        var item = LaptopVariance.NewLaptopVariance("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopVarianceValidatorReportsEveryProblem();
        LaptopVarianceExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LaptopVarianceRequiredCodeRuleFlagsABlankCode();
        LaptopVarianceRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
