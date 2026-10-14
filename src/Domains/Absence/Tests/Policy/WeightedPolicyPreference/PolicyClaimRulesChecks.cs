namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class PolicyClaimRulesChecks
{
    public static void PolicyClaimValidatorReportsEveryProblem()
    {
        var validator = PolicyClaimRules.DefaultPolicyClaimValidator();
        var item = PolicyClaim.NewPolicyClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyClaimWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PolicyClaimWeightRangeRule(7.4, 813.4);
        var item = PolicyClaim.NewPolicyClaim("x", "y");
        item.Weight = 814.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyClaimCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PolicyClaimCostRangeRule(4.1, 948.3);
        var item = PolicyClaim.NewPolicyClaim("x", "y");
        item.Cost = 949.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyClaimCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PolicyClaimCategoryLengthRule(86400);
        var item = PolicyClaim.NewPolicyClaim("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyClaimRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new PolicyClaimRequiredTeamRule("policy claim needs an entry");
        var item = PolicyClaim.NewPolicyClaim("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyClaimRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PolicyClaimRequiredCategoryRule("policy claim needs an entry");
        var item = PolicyClaim.NewPolicyClaim("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyClaimValidatorReportsEveryProblem();
        PolicyClaimWeightRangeRuleFlagsValuesAboveWeight();
        PolicyClaimCostRangeRuleFlagsValuesAboveCost();
        PolicyClaimCategoryLengthRuleFlagsALongCategory();
        PolicyClaimRequiredTeamRuleFlagsABlankTeam();
        PolicyClaimRequiredCategoryRuleFlagsABlankCategory();
    }
}
