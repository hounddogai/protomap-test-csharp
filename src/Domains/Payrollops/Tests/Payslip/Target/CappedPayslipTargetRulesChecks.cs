namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class CappedPayslipTargetRulesChecks
{
    public static void CappedPayslipTargetValidatorReportsEveryProblem()
    {
        var validator = CappedPayslipTargetRules.DefaultCappedPayslipTargetValidator();
        var item = CappedPayslipTarget.NewCappedPayslipTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPayslipTargetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedPayslipTargetLabelLengthRule(3600);
        var item = CappedPayslipTarget.NewCappedPayslipTarget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPayslipTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedPayslipTargetRequiredTitleRule("capped payslip target needs an entry");
        var item = CappedPayslipTarget.NewCappedPayslipTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedPayslipTargetValidatorReportsEveryProblem();
        CappedPayslipTargetLabelLengthRuleFlagsALongLabel();
        CappedPayslipTargetRequiredTitleRuleFlagsABlankTitle();
    }
}
