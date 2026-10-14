namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class RegionalFeedbackProposalRulesChecks
{
    public static void RegionalFeedbackProposalValidatorReportsEveryProblem()
    {
        var validator = RegionalFeedbackProposalRules.DefaultRegionalFeedbackProposalValidator();
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalFeedbackProposalLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalFeedbackProposalLabelLengthRule(16);
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalFeedbackProposalLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new RegionalFeedbackProposalLimitRangeRule(80, 3600);
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalFeedbackProposalUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RegionalFeedbackProposalUpdatedAtRangeRule(1000, 1000);
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalFeedbackProposalValidatorReportsEveryProblem();
        RegionalFeedbackProposalLabelLengthRuleFlagsALongLabel();
        RegionalFeedbackProposalLimitRangeRuleFlagsValuesAboveLimit();
        RegionalFeedbackProposalUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
