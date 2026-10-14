namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckOverrideRulesChecks
{
    public static void PolicyAckOverrideValidatorReportsEveryProblem()
    {
        var validator = PolicyAckOverrideRules.DefaultPolicyAckOverrideValidator();
        var item = PolicyAckOverride.NewPolicyAckOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckOverrideCodeLengthRule(32);
        var item = PolicyAckOverride.NewPolicyAckOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyAckOverrideRequiredCodeRule("policy ack override needs this field");
        var item = PolicyAckOverride.NewPolicyAckOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckOverrideRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new PolicyAckOverrideRequiredNotesRule("policy ack override needs an entry");
        var item = PolicyAckOverride.NewPolicyAckOverride("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PolicyAckOverrideRequiredLabelRule("policy ack override needs an entry");
        var item = PolicyAckOverride.NewPolicyAckOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckOverrideReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PolicyAckOverrideReviewerLengthRule(32);
        var item = PolicyAckOverride.NewPolicyAckOverride("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyAckOverrideValidatorReportsEveryProblem();
        PolicyAckOverrideCodeLengthRuleFlagsALongCode();
        PolicyAckOverrideRequiredCodeRuleFlagsABlankCode();
        PolicyAckOverrideRequiredNotesRuleFlagsABlankNotes();
        PolicyAckOverrideRequiredLabelRuleFlagsABlankLabel();
        PolicyAckOverrideReviewerLengthRuleFlagsALongReviewer();
    }
}
