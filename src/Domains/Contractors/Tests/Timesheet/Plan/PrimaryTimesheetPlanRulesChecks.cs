namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PrimaryTimesheetPlanRulesChecks
{
    public static void PrimaryTimesheetPlanValidatorReportsEveryProblem()
    {
        var validator = PrimaryTimesheetPlanRules.DefaultPrimaryTimesheetPlanValidator();
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryTimesheetPlanTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new PrimaryTimesheetPlanTimezoneLengthRule(32);
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryTimesheetPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryTimesheetPlanRequiredCodeRule("primary timesheet plan needs this field");
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryTimesheetPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryTimesheetPlanCodeLengthRule(1000);
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryTimesheetPlanRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new PrimaryTimesheetPlanRequiredGradeRule("primary timesheet plan needs this field");
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryTimesheetPlanValidatorReportsEveryProblem();
        PrimaryTimesheetPlanTimezoneLengthRuleFlagsALongTimezone();
        PrimaryTimesheetPlanRequiredCodeRuleFlagsABlankCode();
        PrimaryTimesheetPlanCodeLengthRuleFlagsALongCode();
        PrimaryTimesheetPlanRequiredGradeRuleFlagsABlankGrade();
    }
}
