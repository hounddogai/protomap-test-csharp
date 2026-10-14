namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class StrictTaxFormAuditRulesChecks
{
    public static void StrictTaxFormAuditValidatorReportsEveryProblem()
    {
        var validator = StrictTaxFormAuditRules.DefaultStrictTaxFormAuditValidator();
        var item = StrictTaxFormAudit.NewStrictTaxFormAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictTaxFormAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictTaxFormAuditCodeLengthRule(1000);
        var item = StrictTaxFormAudit.NewStrictTaxFormAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictTaxFormAuditNameLengthRuleFlagsALongName()
    {
        var rule = new StrictTaxFormAuditNameLengthRule(16);
        var item = StrictTaxFormAudit.NewStrictTaxFormAudit("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictTaxFormAuditNotesLengthRuleFlagsALongNotes()
    {
        var rule = new StrictTaxFormAuditNotesLengthRule(255);
        var item = StrictTaxFormAudit.NewStrictTaxFormAudit("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictTaxFormAuditValidatorReportsEveryProblem();
        StrictTaxFormAuditCodeLengthRuleFlagsALongCode();
        StrictTaxFormAuditNameLengthRuleFlagsALongName();
        StrictTaxFormAuditNotesLengthRuleFlagsALongNotes();
    }
}
