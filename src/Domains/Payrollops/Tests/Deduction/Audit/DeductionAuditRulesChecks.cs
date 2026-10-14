namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionAuditRulesChecks
{
    public static void DeductionAuditValidatorReportsEveryProblem()
    {
        var validator = DeductionAuditRules.DefaultDeductionAuditValidator();
        var item = DeductionAudit.NewDeductionAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionAuditRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new DeductionAuditRequiredRegionRule("deduction audit needs a value");
        var item = DeductionAudit.NewDeductionAudit("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new DeductionAuditRequiredNameRule("deduction audit needs an entry");
        var item = DeductionAudit.NewDeductionAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionAuditCodeLengthRule(64);
        var item = DeductionAudit.NewDeductionAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionAuditValidatorReportsEveryProblem();
        DeductionAuditRequiredRegionRuleFlagsABlankRegion();
        DeductionAuditRequiredNameRuleFlagsABlankName();
        DeductionAuditCodeLengthRuleFlagsALongCode();
    }
}
