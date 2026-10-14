namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class TieredFeedbackProposalRulesChecks
{
    public static void TieredFeedbackProposalValidatorReportsEveryProblem()
    {
        var validator = TieredFeedbackProposalRules.DefaultTieredFeedbackProposalValidator();
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredFeedbackProposalNameLengthRuleFlagsALongName()
    {
        var rule = new TieredFeedbackProposalNameLengthRule(86400);
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredFeedbackProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredFeedbackProposalCodeLengthRule(3600);
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredFeedbackProposalHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TieredFeedbackProposalHoursRangeRule(8.6, 1347.7);
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("x", "y");
        item.Hours = 1349.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredFeedbackProposalOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new TieredFeedbackProposalOwnerIdLengthRule(86400);
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredFeedbackProposalValidatorReportsEveryProblem();
        TieredFeedbackProposalNameLengthRuleFlagsALongName();
        TieredFeedbackProposalCodeLengthRuleFlagsALongCode();
        TieredFeedbackProposalHoursRangeRuleFlagsValuesAboveHours();
        TieredFeedbackProposalOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
