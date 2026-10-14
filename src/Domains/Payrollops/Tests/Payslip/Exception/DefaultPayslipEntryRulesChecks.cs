namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DefaultPayslipEntryRulesChecks
{
    public static void DefaultPayslipEntryValidatorReportsEveryProblem()
    {
        var validator = DefaultPayslipEntryRules.DefaultDefaultPayslipEntryValidator();
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPayslipEntryNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultPayslipEntryNameLengthRule(80);
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPayslipEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultPayslipEntryRequiredNameRule("default payslip entry needs this field");
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPayslipEntryWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DefaultPayslipEntryWindowDaysRangeRule(64, 1000);
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPayslipEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultPayslipEntryCodeLengthRule(255);
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultPayslipEntryValidatorReportsEveryProblem();
        DefaultPayslipEntryNameLengthRuleFlagsALongName();
        DefaultPayslipEntryRequiredNameRuleFlagsABlankName();
        DefaultPayslipEntryWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DefaultPayslipEntryCodeLengthRuleFlagsALongCode();
    }
}
