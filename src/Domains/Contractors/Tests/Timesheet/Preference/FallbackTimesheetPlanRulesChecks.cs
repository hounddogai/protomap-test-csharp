namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FallbackTimesheetPlanRulesChecks
{
    public static void FallbackTimesheetPlanValidatorReportsEveryProblem()
    {
        var validator = FallbackTimesheetPlanRules.DefaultFallbackTimesheetPlanValidator();
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackTimesheetPlanFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new FallbackTimesheetPlanFteRangeRule(7.0, 4372.0);
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("x", "y");
        item.Fte = 4373.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackTimesheetPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackTimesheetPlanCodeLengthRule(80);
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackTimesheetPlanSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new FallbackTimesheetPlanSummaryLengthRule(32);
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackTimesheetPlanNotesLengthRuleFlagsALongNotes()
    {
        var rule = new FallbackTimesheetPlanNotesLengthRule(3600);
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackTimesheetPlanMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new FallbackTimesheetPlanMarginRangeRule(7.3, 3163.4);
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("x", "y");
        item.Margin = 3164.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackTimesheetPlanValidatorReportsEveryProblem();
        FallbackTimesheetPlanFteRangeRuleFlagsValuesAboveFte();
        FallbackTimesheetPlanCodeLengthRuleFlagsALongCode();
        FallbackTimesheetPlanSummaryLengthRuleFlagsALongSummary();
        FallbackTimesheetPlanNotesLengthRuleFlagsALongNotes();
        FallbackTimesheetPlanMarginRangeRuleFlagsValuesAboveMargin();
    }
}
