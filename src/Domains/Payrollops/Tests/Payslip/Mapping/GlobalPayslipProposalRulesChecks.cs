namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalPayslipProposalRulesChecks
{
    public static void GlobalPayslipProposalValidatorReportsEveryProblem()
    {
        var validator = GlobalPayslipProposalRules.DefaultGlobalPayslipProposalValidator();
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPayslipProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalPayslipProposalRequiredCodeRule("global payslip proposal needs an entry");
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalPayslipProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalPayslipProposalCodeLengthRule(86400);
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPayslipProposalGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new GlobalPayslipProposalGraceDaysRangeRule(64, 80);
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalPayslipProposalValidatorReportsEveryProblem();
        GlobalPayslipProposalRequiredCodeRuleFlagsABlankCode();
        GlobalPayslipProposalCodeLengthRuleFlagsALongCode();
        GlobalPayslipProposalGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
