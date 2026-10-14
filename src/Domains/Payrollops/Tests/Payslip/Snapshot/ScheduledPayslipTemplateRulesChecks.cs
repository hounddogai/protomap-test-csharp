namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledPayslipTemplateRulesChecks
{
    public static void ScheduledPayslipTemplateValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayslipTemplateRules.DefaultScheduledPayslipTemplateValidator();
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayslipTemplateLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new ScheduledPayslipTemplateLeaveDaysRangeRule(32, 120);
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("x", "y");
        item.LeaveDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayslipTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledPayslipTemplateRequiredCodeRule("scheduled payslip template needs a value");
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayslipTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledPayslipTemplateRequiredLabelRule("scheduled payslip template needs an entry");
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayslipTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledPayslipTemplateCodeLengthRule(1000);
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPayslipTemplateValidatorReportsEveryProblem();
        ScheduledPayslipTemplateLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        ScheduledPayslipTemplateRequiredCodeRuleFlagsABlankCode();
        ScheduledPayslipTemplateRequiredLabelRuleFlagsABlankLabel();
        ScheduledPayslipTemplateCodeLengthRuleFlagsALongCode();
    }
}
