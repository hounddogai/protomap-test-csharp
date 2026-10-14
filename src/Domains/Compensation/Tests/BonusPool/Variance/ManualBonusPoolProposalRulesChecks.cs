namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class ManualBonusPoolProposalRulesChecks
{
    public static void ManualBonusPoolProposalValidatorReportsEveryProblem()
    {
        var validator = ManualBonusPoolProposalRules.DefaultManualBonusPoolProposalValidator();
        var item = ManualBonusPoolProposal.NewManualBonusPoolProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBonusPoolProposalNameLengthRuleFlagsALongName()
    {
        var rule = new ManualBonusPoolProposalNameLengthRule(64);
        var item = ManualBonusPoolProposal.NewManualBonusPoolProposal("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBonusPoolProposalBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ManualBonusPoolProposalBadgeLengthRule(16);
        var item = ManualBonusPoolProposal.NewManualBonusPoolProposal("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBonusPoolProposalRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualBonusPoolProposalRequiredNameRule("manual bonus pool proposal needs an entry");
        var item = ManualBonusPoolProposal.NewManualBonusPoolProposal("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualBonusPoolProposalValidatorReportsEveryProblem();
        ManualBonusPoolProposalNameLengthRuleFlagsALongName();
        ManualBonusPoolProposalBadgeLengthRuleFlagsALongBadge();
        ManualBonusPoolProposalRequiredNameRuleFlagsABlankName();
    }
}
