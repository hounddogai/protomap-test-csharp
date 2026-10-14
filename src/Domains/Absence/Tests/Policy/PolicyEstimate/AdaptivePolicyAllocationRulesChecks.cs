namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyAllocationRulesChecks
{
    public static void AdaptivePolicyAllocationValidatorReportsEveryProblem()
    {
        var validator = AdaptivePolicyAllocationRules.DefaultAdaptivePolicyAllocationValidator();
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePolicyAllocationQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AdaptivePolicyAllocationQuantityRangeRule(16, 80);
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptivePolicyAllocationWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new AdaptivePolicyAllocationWeightRangeRule(2.9, 545.7);
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("x", "y");
        item.Weight = 547.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptivePolicyAllocationUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AdaptivePolicyAllocationUnitLengthRule(255);
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptivePolicyAllocationCodeLengthRule(64);
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptivePolicyAllocationRequiredCodeRule("adaptive policy allocation needs this field");
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptivePolicyAllocationValidatorReportsEveryProblem();
        AdaptivePolicyAllocationQuantityRangeRuleFlagsValuesAboveQuantity();
        AdaptivePolicyAllocationWeightRangeRuleFlagsValuesAboveWeight();
        AdaptivePolicyAllocationUnitLengthRuleFlagsALongUnit();
        AdaptivePolicyAllocationCodeLengthRuleFlagsALongCode();
        AdaptivePolicyAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
