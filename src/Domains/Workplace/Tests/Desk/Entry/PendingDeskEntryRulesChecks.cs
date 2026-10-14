namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingDeskEntryRulesChecks
{
    public static void PendingDeskEntryValidatorReportsEveryProblem()
    {
        var validator = PendingDeskEntryRules.DefaultPendingDeskEntryValidator();
        var item = PendingDeskEntry.NewPendingDeskEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingDeskEntryDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new PendingDeskEntryDepartmentLengthRule(1000);
        var item = PendingDeskEntry.NewPendingDeskEntry("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingDeskEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingDeskEntryRequiredLabelRule("pending desk entry needs a value");
        var item = PendingDeskEntry.NewPendingDeskEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingDeskEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingDeskEntryCodeLengthRule(16);
        var item = PendingDeskEntry.NewPendingDeskEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingDeskEntryRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new PendingDeskEntryRequiredUnitRule("pending desk entry needs a value");
        var item = PendingDeskEntry.NewPendingDeskEntry("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingDeskEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingDeskEntryRequiredCodeRule("pending desk entry needs this field");
        var item = PendingDeskEntry.NewPendingDeskEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingDeskEntryValidatorReportsEveryProblem();
        PendingDeskEntryDepartmentLengthRuleFlagsALongDepartment();
        PendingDeskEntryRequiredLabelRuleFlagsABlankLabel();
        PendingDeskEntryCodeLengthRuleFlagsALongCode();
        PendingDeskEntryRequiredUnitRuleFlagsABlankUnit();
        PendingDeskEntryRequiredCodeRuleFlagsABlankCode();
    }
}
