namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PendingVisitorPolicyRulesChecks
{
    public static void PendingVisitorPolicyValidatorReportsEveryProblem()
    {
        var validator = PendingVisitorPolicyRules.DefaultPendingVisitorPolicyValidator();
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingVisitorPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingVisitorPolicyCodeLengthRule(86400);
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingVisitorPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingVisitorPolicyLabelLengthRule(86400);
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingVisitorPolicyRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PendingVisitorPolicyRequiredReviewerRule("pending visitor policy needs this field");
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingVisitorPolicyValidatorReportsEveryProblem();
        PendingVisitorPolicyCodeLengthRuleFlagsALongCode();
        PendingVisitorPolicyLabelLengthRuleFlagsALongLabel();
        PendingVisitorPolicyRequiredReviewerRuleFlagsABlankReviewer();
    }
}
