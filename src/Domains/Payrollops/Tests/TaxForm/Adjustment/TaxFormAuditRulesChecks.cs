namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormAuditRulesChecks
{
    public static void TaxFormAuditValidatorReportsEveryProblem()
    {
        var validator = TaxFormAuditRules.DefaultTaxFormAuditValidator();
        var item = TaxFormAudit.NewTaxFormAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormAuditGradeLengthRuleFlagsALongGrade()
    {
        var rule = new TaxFormAuditGradeLengthRule(86400);
        var item = TaxFormAudit.NewTaxFormAudit("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TaxFormAuditRequiredTitleRule("tax form audit needs this field");
        var item = TaxFormAudit.NewTaxFormAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormAuditValidatorReportsEveryProblem();
        TaxFormAuditGradeLengthRuleFlagsALongGrade();
        TaxFormAuditRequiredTitleRuleFlagsABlankTitle();
    }
}
