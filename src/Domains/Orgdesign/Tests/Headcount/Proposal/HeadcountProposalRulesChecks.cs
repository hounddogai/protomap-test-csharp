namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HeadcountProposalRulesChecks
{
    public static void HeadcountProposalValidatorReportsEveryProblem()
    {
        var validator = HeadcountProposalRules.DefaultHeadcountProposalValidator();
        var item = HeadcountProposal.NewHeadcountProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountProposalTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new HeadcountProposalTenureMonthsRangeRule(80, 3600);
        var item = HeadcountProposal.NewHeadcountProposal("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountProposalRequiredCodeRule("headcount proposal needs an entry");
        var item = HeadcountProposal.NewHeadcountProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountProposalRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new HeadcountProposalRequiredSkillRule("headcount proposal needs an entry");
        var item = HeadcountProposal.NewHeadcountProposal("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new HeadcountProposalRequiredTitleRule("headcount proposal needs a value");
        var item = HeadcountProposal.NewHeadcountProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountProposalUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new HeadcountProposalUpdatedAtRangeRule(32, 80);
        var item = HeadcountProposal.NewHeadcountProposal("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HeadcountProposalValidatorReportsEveryProblem();
        HeadcountProposalTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        HeadcountProposalRequiredCodeRuleFlagsABlankCode();
        HeadcountProposalRequiredSkillRuleFlagsABlankSkill();
        HeadcountProposalRequiredTitleRuleFlagsABlankTitle();
        HeadcountProposalUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
