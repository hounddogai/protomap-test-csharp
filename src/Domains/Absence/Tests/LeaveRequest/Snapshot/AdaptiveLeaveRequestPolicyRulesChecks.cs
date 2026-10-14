namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AdaptiveLeaveRequestPolicyRulesChecks
{
    public static void AdaptiveLeaveRequestPolicyValidatorReportsEveryProblem()
    {
        var validator = AdaptiveLeaveRequestPolicyRules.DefaultAdaptiveLeaveRequestPolicyValidator();
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveLeaveRequestPolicyMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AdaptiveLeaveRequestPolicyMinimumRangeRule(9.6, 493.6);
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("x", "y");
        item.Minimum = 495.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLeaveRequestPolicyRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveLeaveRequestPolicyRequiredNameRule("adaptive leave request policy needs a value");
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveLeaveRequestPolicyCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new AdaptiveLeaveRequestPolicyCategoryLengthRule(3600);
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveLeaveRequestPolicyJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new AdaptiveLeaveRequestPolicyJobCodeLengthRule(16);
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveLeaveRequestPolicyRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new AdaptiveLeaveRequestPolicyRetriesRangeRule(16, 86400);
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveLeaveRequestPolicyValidatorReportsEveryProblem();
        AdaptiveLeaveRequestPolicyMinimumRangeRuleFlagsValuesAboveMinimum();
        AdaptiveLeaveRequestPolicyRequiredNameRuleFlagsABlankName();
        AdaptiveLeaveRequestPolicyCategoryLengthRuleFlagsALongCategory();
        AdaptiveLeaveRequestPolicyJobCodeLengthRuleFlagsALongJobCode();
        AdaptiveLeaveRequestPolicyRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
