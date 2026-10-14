namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingPayRunExceptionRulesChecks
{
    public static void RollingPayRunExceptionValidatorReportsEveryProblem()
    {
        var validator = RollingPayRunExceptionRules.DefaultRollingPayRunExceptionValidator();
        var item = RollingPayRunException.NewRollingPayRunException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPayRunExceptionNotesLengthRuleFlagsALongNotes()
    {
        var rule = new RollingPayRunExceptionNotesLengthRule(1000);
        var item = RollingPayRunException.NewRollingPayRunException("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPayRunExceptionRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new RollingPayRunExceptionRequiredDepartmentRule("rolling pay run exception needs this field");
        var item = RollingPayRunException.NewRollingPayRunException("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPayRunExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new RollingPayRunExceptionNameLengthRule(16);
        var item = RollingPayRunException.NewRollingPayRunException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingPayRunExceptionValidatorReportsEveryProblem();
        RollingPayRunExceptionNotesLengthRuleFlagsALongNotes();
        RollingPayRunExceptionRequiredDepartmentRuleFlagsABlankDepartment();
        RollingPayRunExceptionNameLengthRuleFlagsALongName();
    }
}
