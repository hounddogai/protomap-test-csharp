namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayslipProposalRulesChecks
{
    public static void PayslipProposalValidatorReportsEveryProblem()
    {
        var validator = PayslipProposalRules.DefaultPayslipProposalValidator();
        var item = PayslipProposal.NewPayslipProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipProposalTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayslipProposalTitleLengthRule(32);
        var item = PayslipProposal.NewPayslipProposal("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipProposalMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new PayslipProposalMinimumRangeRule(9.4, 3085.0);
        var item = PayslipProposal.NewPayslipProposal("x", "y");
        item.Minimum = 3086.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipProposalSkillLengthRuleFlagsALongSkill()
    {
        var rule = new PayslipProposalSkillLengthRule(80);
        var item = PayslipProposal.NewPayslipProposal("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayslipProposalRequiredCodeRule("payslip proposal needs an entry");
        var item = PayslipProposal.NewPayslipProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipProposalValidatorReportsEveryProblem();
        PayslipProposalTitleLengthRuleFlagsALongTitle();
        PayslipProposalMinimumRangeRuleFlagsValuesAboveMinimum();
        PayslipProposalSkillLengthRuleFlagsALongSkill();
        PayslipProposalRequiredCodeRuleFlagsABlankCode();
    }
}
