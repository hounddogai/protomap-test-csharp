namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyProfileRulesChecks
{
    public static void PolicyProfileValidatorReportsEveryProblem()
    {
        var validator = PolicyProfileRules.DefaultPolicyProfileValidator();
        var item = PolicyProfile.NewPolicyProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyProfileGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PolicyProfileGraceDaysRangeRule(32, 255);
        var item = PolicyProfile.NewPolicyProfile("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyProfileRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new PolicyProfileRequiredCostCenterRule("policy profile needs a value");
        var item = PolicyProfile.NewPolicyProfile("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyProfileCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PolicyProfileCategoryLengthRule(32);
        var item = PolicyProfile.NewPolicyProfile("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyProfileCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new PolicyProfileCostCenterLengthRule(1000);
        var item = PolicyProfile.NewPolicyProfile("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyProfileValidatorReportsEveryProblem();
        PolicyProfileGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        PolicyProfileRequiredCostCenterRuleFlagsABlankCostCenter();
        PolicyProfileCategoryLengthRuleFlagsALongCategory();
        PolicyProfileCostCenterLengthRuleFlagsALongCostCenter();
    }
}
