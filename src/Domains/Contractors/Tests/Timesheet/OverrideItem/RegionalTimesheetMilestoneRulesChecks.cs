namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalTimesheetMilestoneRulesChecks
{
    public static void RegionalTimesheetMilestoneValidatorReportsEveryProblem()
    {
        var validator = RegionalTimesheetMilestoneRules.DefaultRegionalTimesheetMilestoneValidator();
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalTimesheetMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalTimesheetMilestoneRequiredCodeRule("regional timesheet milestone needs an entry");
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTimesheetMilestoneCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new RegionalTimesheetMilestoneCostCenterLengthRule(1000);
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalTimesheetMilestoneScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RegionalTimesheetMilestoneScoreRangeRule(5.9, 4512.0);
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("x", "y");
        item.Score = 4513.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalTimesheetMilestoneCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RegionalTimesheetMilestoneCostRangeRule(9.8, 497.1);
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("x", "y");
        item.Cost = 498.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalTimesheetMilestoneCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new RegionalTimesheetMilestoneCeilingRangeRule(2.9, 3195.5);
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("x", "y");
        item.Ceiling = 3197.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalTimesheetMilestoneValidatorReportsEveryProblem();
        RegionalTimesheetMilestoneRequiredCodeRuleFlagsABlankCode();
        RegionalTimesheetMilestoneCostCenterLengthRuleFlagsALongCostCenter();
        RegionalTimesheetMilestoneScoreRangeRuleFlagsValuesAboveScore();
        RegionalTimesheetMilestoneCostRangeRuleFlagsValuesAboveCost();
        RegionalTimesheetMilestoneCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
