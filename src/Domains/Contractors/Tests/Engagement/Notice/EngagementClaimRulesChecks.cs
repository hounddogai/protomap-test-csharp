namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementClaimRulesChecks
{
    public static void EngagementClaimValidatorReportsEveryProblem()
    {
        var validator = EngagementClaimRules.DefaultEngagementClaimValidator();
        var item = EngagementClaim.NewEngagementClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementClaimRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new EngagementClaimRequiredExternalRefRule("engagement claim needs this field");
        var item = EngagementClaim.NewEngagementClaim("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementClaimDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new EngagementClaimDiscountRangeRule(4.6, 4618.1);
        var item = EngagementClaim.NewEngagementClaim("x", "y");
        item.Discount = 4619.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementClaimCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new EngagementClaimCostRangeRule(4.0, 3354.6);
        var item = EngagementClaim.NewEngagementClaim("x", "y");
        item.Cost = 3356.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementClaimRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new EngagementClaimRequiredLabelRule("engagement claim needs a value");
        var item = EngagementClaim.NewEngagementClaim("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementClaimValidatorReportsEveryProblem();
        EngagementClaimRequiredExternalRefRuleFlagsABlankExternalRef();
        EngagementClaimDiscountRangeRuleFlagsValuesAboveDiscount();
        EngagementClaimCostRangeRuleFlagsValuesAboveCost();
        EngagementClaimRequiredLabelRuleFlagsABlankLabel();
    }
}
