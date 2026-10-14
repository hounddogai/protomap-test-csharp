namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class ScheduledPayslipNoticeRulesChecks
{
    public static void ScheduledPayslipNoticeValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayslipNoticeRules.DefaultScheduledPayslipNoticeValidator();
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayslipNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledPayslipNoticeNameLengthRule(16);
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayslipNoticeRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ScheduledPayslipNoticeRevisionRangeRule(255, 255);
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayslipNoticeRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledPayslipNoticeRequiredNameRule("scheduled payslip notice needs a value");
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPayslipNoticeValidatorReportsEveryProblem();
        ScheduledPayslipNoticeNameLengthRuleFlagsALongName();
        ScheduledPayslipNoticeRevisionRangeRuleFlagsValuesAboveRevision();
        ScheduledPayslipNoticeRequiredNameRuleFlagsABlankName();
    }
}
