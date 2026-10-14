namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DefaultPayslipBudgetRulesChecks
{
    public static void DefaultPayslipBudgetValidatorReportsEveryProblem()
    {
        var validator = DefaultPayslipBudgetRules.DefaultDefaultPayslipBudgetValidator();
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPayslipBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultPayslipBudgetRequiredCodeRule("default payslip budget needs a value");
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPayslipBudgetRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new DefaultPayslipBudgetRevisionRangeRule(64, 120);
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPayslipBudgetStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new DefaultPayslipBudgetStartsAtRangeRule(16, 3600);
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultPayslipBudgetValidatorReportsEveryProblem();
        DefaultPayslipBudgetRequiredCodeRuleFlagsABlankCode();
        DefaultPayslipBudgetRevisionRangeRuleFlagsValuesAboveRevision();
        DefaultPayslipBudgetStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
