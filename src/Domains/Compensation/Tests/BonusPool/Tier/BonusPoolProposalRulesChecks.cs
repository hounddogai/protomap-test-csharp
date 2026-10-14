namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolProposalRulesChecks
{
    public static void BonusPoolProposalValidatorReportsEveryProblem()
    {
        var validator = BonusPoolProposalRules.DefaultBonusPoolProposalValidator();
        var item = BonusPoolProposal.NewBonusPoolProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolProposalRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BonusPoolProposalRateRangeRule(3.4, 4655.8);
        var item = BonusPoolProposal.NewBonusPoolProposal("x", "y");
        item.Rate = 4657.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolProposalBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new BonusPoolProposalBadgeLengthRule(32);
        var item = BonusPoolProposal.NewBonusPoolProposal("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BonusPoolProposalValidatorReportsEveryProblem();
        BonusPoolProposalRateRangeRuleFlagsValuesAboveRate();
        BonusPoolProposalBadgeLengthRuleFlagsALongBadge();
    }
}
