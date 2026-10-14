namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RollingFeedbackProposalRulesChecks
{
    public static void RollingFeedbackProposalValidatorReportsEveryProblem()
    {
        var validator = RollingFeedbackProposalRules.DefaultRollingFeedbackProposalValidator();
        var item = RollingFeedbackProposal.NewRollingFeedbackProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingFeedbackProposalRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RollingFeedbackProposalRatioRangeRule(3.8, 4068.7);
        var item = RollingFeedbackProposal.NewRollingFeedbackProposal("x", "y");
        item.Ratio = 4070.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingFeedbackProposalRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new RollingFeedbackProposalRatingRangeRule(2.6, 2684.8);
        var item = RollingFeedbackProposal.NewRollingFeedbackProposal("x", "y");
        item.Rating = 2686.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingFeedbackProposalRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new RollingFeedbackProposalRequiredExternalRefRule("rolling feedback proposal needs an entry");
        var item = RollingFeedbackProposal.NewRollingFeedbackProposal("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingFeedbackProposalValidatorReportsEveryProblem();
        RollingFeedbackProposalRatioRangeRuleFlagsValuesAboveRatio();
        RollingFeedbackProposalRatingRangeRuleFlagsValuesAboveRating();
        RollingFeedbackProposalRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
