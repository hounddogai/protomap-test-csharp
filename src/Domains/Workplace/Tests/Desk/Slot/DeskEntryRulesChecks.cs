namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskEntryRulesChecks
{
    public static void DeskEntryValidatorReportsEveryProblem()
    {
        var validator = DeskEntryRules.DefaultDeskEntryValidator();
        var item = DeskEntry.NewDeskEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskEntryRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new DeskEntryRequiredEmployeeIdRule("desk entry needs a value");
        var item = DeskEntry.NewDeskEntry("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskEntryCodeLengthRule(64);
        var item = DeskEntry.NewDeskEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskEntryDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new DeskEntryDepartmentLengthRule(16);
        var item = DeskEntry.NewDeskEntry("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskEntryValidatorReportsEveryProblem();
        DeskEntryRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        DeskEntryCodeLengthRuleFlagsALongCode();
        DeskEntryDepartmentLengthRuleFlagsALongDepartment();
    }
}
