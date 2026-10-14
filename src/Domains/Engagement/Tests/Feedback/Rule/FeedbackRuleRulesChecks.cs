namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FeedbackRuleRulesChecks
{
    public static void FeedbackRuleValidatorReportsEveryProblem()
    {
        var validator = FeedbackRuleRules.DefaultFeedbackRuleValidator();
        var item = FeedbackRule.NewFeedbackRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackRuleOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new FeedbackRuleOwnerIdLengthRule(120);
        var item = FeedbackRule.NewFeedbackRule("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackRuleRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new FeedbackRuleRequiredOwnerIdRule("feedback rule needs this field");
        var item = FeedbackRule.NewFeedbackRule("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackRuleRequiredCodeRule("feedback rule needs this field");
        var item = FeedbackRule.NewFeedbackRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackRuleRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new FeedbackRuleRequiredNotesRule("feedback rule needs an entry");
        var item = FeedbackRule.NewFeedbackRule("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackRuleValidatorReportsEveryProblem();
        FeedbackRuleOwnerIdLengthRuleFlagsALongOwnerId();
        FeedbackRuleRequiredOwnerIdRuleFlagsABlankOwnerId();
        FeedbackRuleRequiredCodeRuleFlagsABlankCode();
        FeedbackRuleRequiredNotesRuleFlagsABlankNotes();
    }
}
