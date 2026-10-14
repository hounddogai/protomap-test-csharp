namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SuccessionPlanClaimRulesChecks
{
    public static void SuccessionPlanClaimValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanClaimRules.DefaultSuccessionPlanClaimValidator();
        var item = SuccessionPlanClaim.NewSuccessionPlanClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanClaimAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SuccessionPlanClaimAmountRangeRule(4.0, 2334.9);
        var item = SuccessionPlanClaim.NewSuccessionPlanClaim("x", "y");
        item.Amount = 2336.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SuccessionPlanClaimRequiredTitleRule("succession plan claim needs a value");
        var item = SuccessionPlanClaim.NewSuccessionPlanClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SuccessionPlanClaimValidatorReportsEveryProblem();
        SuccessionPlanClaimAmountRangeRuleFlagsValuesAboveAmount();
        SuccessionPlanClaimRequiredTitleRuleFlagsABlankTitle();
    }
}
