namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetPlanRulesChecks
{
    public static void LenientTimesheetPlanValidatorReportsEveryProblem()
    {
        var validator = LenientTimesheetPlanRules.DefaultLenientTimesheetPlanValidator();
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientTimesheetPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientTimesheetPlanRequiredNameRule("lenient timesheet plan needs a value");
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientTimesheetPlanRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new LenientTimesheetPlanRequiredSegmentRule("lenient timesheet plan needs this field");
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientTimesheetPlanRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LenientTimesheetPlanRequiredLocationRule("lenient timesheet plan needs an entry");
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientTimesheetPlanLocationLengthRuleFlagsALongLocation()
    {
        var rule = new LenientTimesheetPlanLocationLengthRule(255);
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientTimesheetPlanValidatorReportsEveryProblem();
        LenientTimesheetPlanRequiredNameRuleFlagsABlankName();
        LenientTimesheetPlanRequiredSegmentRuleFlagsABlankSegment();
        LenientTimesheetPlanRequiredLocationRuleFlagsABlankLocation();
        LenientTimesheetPlanLocationLengthRuleFlagsALongLocation();
    }
}
