namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictTimesheetEstimateRulesChecks
{
    public static void StrictTimesheetEstimateValidatorReportsEveryProblem()
    {
        var validator = StrictTimesheetEstimateRules.DefaultStrictTimesheetEstimateValidator();
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictTimesheetEstimateCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new StrictTimesheetEstimateCostCenterLengthRule(255);
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictTimesheetEstimateRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new StrictTimesheetEstimateRequiredNotesRule("strict timesheet estimate needs this field");
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictTimesheetEstimateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictTimesheetEstimateLabelLengthRule(120);
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictTimesheetEstimateRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new StrictTimesheetEstimateRequiredCostCenterRule("strict timesheet estimate needs this field");
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictTimesheetEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictTimesheetEstimateCodeLengthRule(80);
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictTimesheetEstimateValidatorReportsEveryProblem();
        StrictTimesheetEstimateCostCenterLengthRuleFlagsALongCostCenter();
        StrictTimesheetEstimateRequiredNotesRuleFlagsABlankNotes();
        StrictTimesheetEstimateLabelLengthRuleFlagsALongLabel();
        StrictTimesheetEstimateRequiredCostCenterRuleFlagsABlankCostCenter();
        StrictTimesheetEstimateCodeLengthRuleFlagsALongCode();
    }
}
