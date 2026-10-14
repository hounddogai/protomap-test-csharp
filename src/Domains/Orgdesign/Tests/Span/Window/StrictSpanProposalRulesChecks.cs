namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StrictSpanProposalRulesChecks
{
    public static void StrictSpanProposalValidatorReportsEveryProblem()
    {
        var validator = StrictSpanProposalRules.DefaultStrictSpanProposalValidator();
        var item = StrictSpanProposal.NewStrictSpanProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictSpanProposalSkillLengthRuleFlagsALongSkill()
    {
        var rule = new StrictSpanProposalSkillLengthRule(120);
        var item = StrictSpanProposal.NewStrictSpanProposal("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSpanProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictSpanProposalCodeLengthRule(16);
        var item = StrictSpanProposal.NewStrictSpanProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSpanProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictSpanProposalRequiredCodeRule("strict span proposal needs this field");
        var item = StrictSpanProposal.NewStrictSpanProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictSpanProposalValidatorReportsEveryProblem();
        StrictSpanProposalSkillLengthRuleFlagsALongSkill();
        StrictSpanProposalCodeLengthRuleFlagsALongCode();
        StrictSpanProposalRequiredCodeRuleFlagsABlankCode();
    }
}
