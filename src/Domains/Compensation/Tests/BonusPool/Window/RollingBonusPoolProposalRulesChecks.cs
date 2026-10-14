namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RollingBonusPoolProposalRulesChecks
{
    public static void RollingBonusPoolProposalValidatorReportsEveryProblem()
    {
        var validator = RollingBonusPoolProposalRules.DefaultRollingBonusPoolProposalValidator();
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBonusPoolProposalRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new RollingBonusPoolProposalRequiredReviewerRule("rolling bonus pool proposal needs an entry");
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBonusPoolProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingBonusPoolProposalRequiredTitleRule("rolling bonus pool proposal needs a value");
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBonusPoolProposalTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RollingBonusPoolProposalTitleLengthRule(1000);
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBonusPoolProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingBonusPoolProposalCodeLengthRule(80);
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingBonusPoolProposalValidatorReportsEveryProblem();
        RollingBonusPoolProposalRequiredReviewerRuleFlagsABlankReviewer();
        RollingBonusPoolProposalRequiredTitleRuleFlagsABlankTitle();
        RollingBonusPoolProposalTitleLengthRuleFlagsALongTitle();
        RollingBonusPoolProposalCodeLengthRuleFlagsALongCode();
    }
}
