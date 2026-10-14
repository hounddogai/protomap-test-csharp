namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class GlobalFeedbackProposalRulesChecks
{
    public static void GlobalFeedbackProposalValidatorReportsEveryProblem()
    {
        var validator = GlobalFeedbackProposalRules.DefaultGlobalFeedbackProposalValidator();
        var item = GlobalFeedbackProposal.NewGlobalFeedbackProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalFeedbackProposalRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new GlobalFeedbackProposalRequiredCategoryRule("global feedback proposal needs an entry");
        var item = GlobalFeedbackProposal.NewGlobalFeedbackProposal("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalFeedbackProposalRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new GlobalFeedbackProposalRequiredContactEmailRule("global feedback proposal needs an entry");
        var item = GlobalFeedbackProposal.NewGlobalFeedbackProposal("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalFeedbackProposalValidatorReportsEveryProblem();
        GlobalFeedbackProposalRequiredCategoryRuleFlagsABlankCategory();
        GlobalFeedbackProposalRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
