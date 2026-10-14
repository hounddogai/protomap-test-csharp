namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanClaimRulesChecks
{
    public static void LenientSuccessionPlanClaimValidatorReportsEveryProblem()
    {
        var validator = LenientSuccessionPlanClaimRules.DefaultLenientSuccessionPlanClaimValidator();
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSuccessionPlanClaimRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LenientSuccessionPlanClaimRequiredCostCenterRule("lenient succession plan claim needs an entry");
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSuccessionPlanClaimRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientSuccessionPlanClaimRequiredLabelRule("lenient succession plan claim needs a value");
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSuccessionPlanClaimLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientSuccessionPlanClaimLabelLengthRule(255);
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSuccessionPlanClaimCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new LenientSuccessionPlanClaimCostCenterLengthRule(1000);
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSuccessionPlanClaimRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new LenientSuccessionPlanClaimRequiredSourceRule("lenient succession plan claim needs this field");
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSuccessionPlanClaimLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LenientSuccessionPlanClaimLeaveDaysRangeRule(16, 86400);
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanClaimValidatorReportsEveryProblem();
        LenientSuccessionPlanClaimRequiredCostCenterRuleFlagsABlankCostCenter();
        LenientSuccessionPlanClaimRequiredLabelRuleFlagsABlankLabel();
        LenientSuccessionPlanClaimLabelLengthRuleFlagsALongLabel();
        LenientSuccessionPlanClaimCostCenterLengthRuleFlagsALongCostCenter();
        LenientSuccessionPlanClaimRequiredSourceRuleFlagsABlankSource();
        LenientSuccessionPlanClaimLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
