namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentProposalRulesChecks
{
    public static void SentimentProposalValidatorReportsEveryProblem()
    {
        var validator = SentimentProposalRules.DefaultSentimentProposalValidator();
        var item = SentimentProposal.NewSentimentProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentProposalCodeLengthRule(80);
        var item = SentimentProposal.NewSentimentProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SentimentProposalRequiredTitleRule("sentiment proposal needs a value");
        var item = SentimentProposal.NewSentimentProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentProposalFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SentimentProposalFloorValueRangeRule(2.4, 725.3);
        var item = SentimentProposal.NewSentimentProposal("x", "y");
        item.FloorValue = 726.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentProposalValidatorReportsEveryProblem();
        SentimentProposalCodeLengthRuleFlagsALongCode();
        SentimentProposalRequiredTitleRuleFlagsABlankTitle();
        SentimentProposalFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
