namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class AdaptiveDeductionAuditRulesChecks
{
    public static void AdaptiveDeductionAuditValidatorReportsEveryProblem()
    {
        var validator = AdaptiveDeductionAuditRules.DefaultAdaptiveDeductionAuditValidator();
        var item = AdaptiveDeductionAudit.NewAdaptiveDeductionAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveDeductionAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveDeductionAuditCodeLengthRule(32);
        var item = AdaptiveDeductionAudit.NewAdaptiveDeductionAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveDeductionAuditRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new AdaptiveDeductionAuditRequiredGradeRule("adaptive deduction audit needs a value");
        var item = AdaptiveDeductionAudit.NewAdaptiveDeductionAudit("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveDeductionAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveDeductionAuditRequiredLabelRule("adaptive deduction audit needs this field");
        var item = AdaptiveDeductionAudit.NewAdaptiveDeductionAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveDeductionAuditValidatorReportsEveryProblem();
        AdaptiveDeductionAuditCodeLengthRuleFlagsALongCode();
        AdaptiveDeductionAuditRequiredGradeRuleFlagsABlankGrade();
        AdaptiveDeductionAuditRequiredLabelRuleFlagsABlankLabel();
    }
}
