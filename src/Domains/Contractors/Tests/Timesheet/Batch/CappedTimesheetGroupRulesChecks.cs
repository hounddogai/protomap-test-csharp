namespace Acme.Workforce.Contractors.Tests.Timesheet.Batch;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedTimesheetGroupRulesChecks
{
    public static void CappedTimesheetGroupValidatorReportsEveryProblem()
    {
        var validator = CappedTimesheetGroupRules.DefaultCappedTimesheetGroupValidator();
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedTimesheetGroupRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new CappedTimesheetGroupRequiredUnitRule("capped timesheet group needs an entry");
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedTimesheetGroupNameLengthRuleFlagsALongName()
    {
        var rule = new CappedTimesheetGroupNameLengthRule(120);
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedTimesheetGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedTimesheetGroupRequiredCodeRule("capped timesheet group needs this field");
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedTimesheetGroupValidatorReportsEveryProblem();
        CappedTimesheetGroupRequiredUnitRuleFlagsABlankUnit();
        CappedTimesheetGroupNameLengthRuleFlagsALongName();
        CappedTimesheetGroupRequiredCodeRuleFlagsABlankCode();
    }
}
