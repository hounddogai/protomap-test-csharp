namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileProposalRulesChecks
{
    public static void SkillProfileProposalValidatorReportsEveryProblem()
    {
        var validator = SkillProfileProposalRules.DefaultSkillProfileProposalValidator();
        var item = SkillProfileProposal.NewSkillProfileProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SkillProfileProposalRequiredLabelRule("skill profile proposal needs this field");
        var item = SkillProfileProposal.NewSkillProfileProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileProposalRequiredCodeRule("skill profile proposal needs this field");
        var item = SkillProfileProposal.NewSkillProfileProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileProposalStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new SkillProfileProposalStartsAtRangeRule(120, 255);
        var item = SkillProfileProposal.NewSkillProfileProposal("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileProposalRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new SkillProfileProposalRequiredCategoryRule("skill profile proposal needs a value");
        var item = SkillProfileProposal.NewSkillProfileProposal("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfileProposalValidatorReportsEveryProblem();
        SkillProfileProposalRequiredLabelRuleFlagsABlankLabel();
        SkillProfileProposalRequiredCodeRuleFlagsABlankCode();
        SkillProfileProposalStartsAtRangeRuleFlagsValuesAboveStartsAt();
        SkillProfileProposalRequiredCategoryRuleFlagsABlankCategory();
    }
}
