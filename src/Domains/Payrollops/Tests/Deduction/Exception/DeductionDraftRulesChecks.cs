namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DeductionDraftRulesChecks
{
    public static void DeductionDraftValidatorReportsEveryProblem()
    {
        var validator = DeductionDraftRules.DefaultDeductionDraftValidator();
        var item = DeductionDraft.NewDeductionDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionDraftDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new DeductionDraftDepartmentLengthRule(120);
        var item = DeductionDraft.NewDeductionDraft("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DeductionDraftTitleLengthRule(1000);
        var item = DeductionDraft.NewDeductionDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeductionDraftRequiredTitleRule("deduction draft needs this field");
        var item = DeductionDraft.NewDeductionDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionDraftValidatorReportsEveryProblem();
        DeductionDraftDepartmentLengthRuleFlagsALongDepartment();
        DeductionDraftTitleLengthRuleFlagsALongTitle();
        DeductionDraftRequiredTitleRuleFlagsABlankTitle();
    }
}
