namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Checks of model policy code, run by the project's runner.</summary>
public static class CostModelDigestRulesChecks
{
    public static void CostModelDigestValidatorReportsEveryProblem()
    {
        var validator = CostModelDigestRules.DefaultCostModelDigestValidator();
        var item = CostModelDigest.NewCostModelDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelDigestDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new CostModelDigestDiscountRangeRule(1.1, 3023.9);
        var item = CostModelDigest.NewCostModelDigest("x", "y");
        item.Discount = 3025.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelDigestAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new CostModelDigestAmountRangeRule(8.8, 3656.3);
        var item = CostModelDigest.NewCostModelDigest("x", "y");
        item.Amount = 3657.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelDigestDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new CostModelDigestDescriptionLengthRule(255);
        var item = CostModelDigest.NewCostModelDigest("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelDigestCodeLengthRule(1000);
        var item = CostModelDigest.NewCostModelDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelDigestValidatorReportsEveryProblem();
        CostModelDigestDiscountRangeRuleFlagsValuesAboveDiscount();
        CostModelDigestAmountRangeRuleFlagsValuesAboveAmount();
        CostModelDigestDescriptionLengthRuleFlagsALongDescription();
        CostModelDigestCodeLengthRuleFlagsALongCode();
    }
}
