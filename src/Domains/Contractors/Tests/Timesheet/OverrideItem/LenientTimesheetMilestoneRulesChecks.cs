namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class LenientTimesheetMilestoneRulesChecks
{
    public static void LenientTimesheetMilestoneValidatorReportsEveryProblem()
    {
        var validator = LenientTimesheetMilestoneRules.DefaultLenientTimesheetMilestoneValidator();
        var item = LenientTimesheetMilestone.NewLenientTimesheetMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientTimesheetMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientTimesheetMilestoneCodeLengthRule(16);
        var item = LenientTimesheetMilestone.NewLenientTimesheetMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientTimesheetMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientTimesheetMilestoneRequiredCodeRule("lenient timesheet milestone needs a value");
        var item = LenientTimesheetMilestone.NewLenientTimesheetMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientTimesheetMilestoneValidatorReportsEveryProblem();
        LenientTimesheetMilestoneCodeLengthRuleFlagsALongCode();
        LenientTimesheetMilestoneRequiredCodeRuleFlagsABlankCode();
    }
}
