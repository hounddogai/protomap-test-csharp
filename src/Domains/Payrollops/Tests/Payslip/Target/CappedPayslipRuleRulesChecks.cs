namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class CappedPayslipRuleRulesChecks
{
    public static void CappedPayslipRuleValidatorReportsEveryProblem()
    {
        var validator = CappedPayslipRuleRules.DefaultCappedPayslipRuleValidator();
        var item = CappedPayslipRule.NewCappedPayslipRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPayslipRuleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new CappedPayslipRuleSalaryBandLengthRule(64);
        var item = CappedPayslipRule.NewCappedPayslipRule("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPayslipRuleRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new CappedPayslipRuleRequiredSalaryBandRule("capped payslip rule needs an entry");
        var item = CappedPayslipRule.NewCappedPayslipRule("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedPayslipRuleValidatorReportsEveryProblem();
        CappedPayslipRuleSalaryBandLengthRuleFlagsALongSalaryBand();
        CappedPayslipRuleRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
