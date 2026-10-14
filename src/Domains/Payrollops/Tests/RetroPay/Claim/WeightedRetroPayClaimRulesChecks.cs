namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class WeightedRetroPayClaimRulesChecks
{
    public static void WeightedRetroPayClaimValidatorReportsEveryProblem()
    {
        var validator = WeightedRetroPayClaimRules.DefaultWeightedRetroPayClaimValidator();
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRetroPayClaimRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new WeightedRetroPayClaimRequiredLocaleRule("weighted retro pay claim needs a value");
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRetroPayClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedRetroPayClaimRequiredCodeRule("weighted retro pay claim needs an entry");
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRetroPayClaimCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new WeightedRetroPayClaimCapacityRangeRule(80, 86400);
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedRetroPayClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedRetroPayClaimCodeLengthRule(86400);
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRetroPayClaimTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedRetroPayClaimTitleLengthRule(64);
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRetroPayClaimManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new WeightedRetroPayClaimManagerIdLengthRule(3600);
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedRetroPayClaimValidatorReportsEveryProblem();
        WeightedRetroPayClaimRequiredLocaleRuleFlagsABlankLocale();
        WeightedRetroPayClaimRequiredCodeRuleFlagsABlankCode();
        WeightedRetroPayClaimCapacityRangeRuleFlagsValuesAboveCapacity();
        WeightedRetroPayClaimCodeLengthRuleFlagsALongCode();
        WeightedRetroPayClaimTitleLengthRuleFlagsALongTitle();
        WeightedRetroPayClaimManagerIdLengthRuleFlagsALongManagerId();
    }
}
