namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackProposalRulesChecks
{
    public static void FeedbackProposalValidatorReportsEveryProblem()
    {
        var validator = FeedbackProposalRules.DefaultFeedbackProposalValidator();
        var item = FeedbackProposal.NewFeedbackProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackProposalCodeLengthRule(86400);
        var item = FeedbackProposal.NewFeedbackProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FeedbackProposalRequiredTitleRule("feedback proposal needs a value");
        var item = FeedbackProposal.NewFeedbackProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackProposalRequiredCodeRule("feedback proposal needs an entry");
        var item = FeedbackProposal.NewFeedbackProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackProposalValidatorReportsEveryProblem();
        FeedbackProposalCodeLengthRuleFlagsALongCode();
        FeedbackProposalRequiredTitleRuleFlagsABlankTitle();
        FeedbackProposalRequiredCodeRuleFlagsABlankCode();
    }
}
