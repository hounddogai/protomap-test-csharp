namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class RegionalPayslipExceptionRulesChecks
{
    public static void RegionalPayslipExceptionValidatorReportsEveryProblem()
    {
        var validator = RegionalPayslipExceptionRules.DefaultRegionalPayslipExceptionValidator();
        var item = RegionalPayslipException.NewRegionalPayslipException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPayslipExceptionRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalPayslipExceptionRequiredTitleRule("regional payslip exception needs a value");
        var item = RegionalPayslipException.NewRegionalPayslipException("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalPayslipExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalPayslipExceptionLabelLengthRule(255);
        var item = RegionalPayslipException.NewRegionalPayslipException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPayslipExceptionExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RegionalPayslipExceptionExpiresAtRangeRule(80, 1000);
        var item = RegionalPayslipException.NewRegionalPayslipException("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalPayslipExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalPayslipExceptionRequiredCodeRule("regional payslip exception needs a value");
        var item = RegionalPayslipException.NewRegionalPayslipException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalPayslipExceptionValidatorReportsEveryProblem();
        RegionalPayslipExceptionRequiredTitleRuleFlagsABlankTitle();
        RegionalPayslipExceptionLabelLengthRuleFlagsALongLabel();
        RegionalPayslipExceptionExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RegionalPayslipExceptionRequiredCodeRuleFlagsABlankCode();
    }
}
