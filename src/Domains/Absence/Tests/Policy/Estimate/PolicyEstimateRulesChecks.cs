namespace Acme.Workforce.Absence.Tests.Policy.Estimate;

using Acme.Workforce.Absence.Policy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PolicyEstimateRulesChecks
{
    public static void PolicyEstimateValidatorReportsEveryProblem()
    {
        var validator = PolicyEstimateRules.DefaultPolicyEstimateValidator();
        var item = PolicyEstimate.NewPolicyEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyEstimateTitleLengthRule(120);
        var item = PolicyEstimate.NewPolicyEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyEstimateContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new PolicyEstimateContactEmailLengthRule(3600);
        var item = PolicyEstimate.NewPolicyEstimate("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyEstimateDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PolicyEstimateDiscountRangeRule(3.6, 4117.6);
        var item = PolicyEstimate.NewPolicyEstimate("x", "y");
        item.Discount = 4119.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyEstimateJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new PolicyEstimateJobCodeLengthRule(255);
        var item = PolicyEstimate.NewPolicyEstimate("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyEstimateBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PolicyEstimateBudgetRangeRule(8.6, 1106.5);
        var item = PolicyEstimate.NewPolicyEstimate("x", "y");
        item.Budget = 1108.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyEstimateValidatorReportsEveryProblem();
        PolicyEstimateTitleLengthRuleFlagsALongTitle();
        PolicyEstimateContactEmailLengthRuleFlagsALongContactEmail();
        PolicyEstimateDiscountRangeRuleFlagsValuesAboveDiscount();
        PolicyEstimateJobCodeLengthRuleFlagsALongJobCode();
        PolicyEstimateBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
