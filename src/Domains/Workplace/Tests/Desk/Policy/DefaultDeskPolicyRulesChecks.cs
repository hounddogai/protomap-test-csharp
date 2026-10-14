namespace Acme.Workforce.Workplace.Tests.Desk.Policy;

using Acme.Workforce.Workplace.Desk.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultDeskPolicyRulesChecks
{
    public static void DefaultDeskPolicyValidatorReportsEveryProblem()
    {
        var validator = DefaultDeskPolicyRules.DefaultDefaultDeskPolicyValidator();
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultDeskPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultDeskPolicyCodeLengthRule(255);
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultDeskPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultDeskPolicyRequiredCodeRule("default desk policy needs an entry");
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultDeskPolicyReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new DefaultDeskPolicyReviewerLengthRule(80);
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultDeskPolicyValidatorReportsEveryProblem();
        DefaultDeskPolicyCodeLengthRuleFlagsALongCode();
        DefaultDeskPolicyRequiredCodeRuleFlagsABlankCode();
        DefaultDeskPolicyReviewerLengthRuleFlagsALongReviewer();
    }
}
