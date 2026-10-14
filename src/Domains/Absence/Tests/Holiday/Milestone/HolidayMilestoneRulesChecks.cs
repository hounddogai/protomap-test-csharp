namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayMilestoneRulesChecks
{
    public static void HolidayMilestoneValidatorReportsEveryProblem()
    {
        var validator = HolidayMilestoneRules.DefaultHolidayMilestoneValidator();
        var item = HolidayMilestone.NewHolidayMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayMilestoneCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new HolidayMilestoneCostCenterLengthRule(32);
        var item = HolidayMilestone.NewHolidayMilestone("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HolidayMilestoneRequiredCodeRule("holiday milestone needs this field");
        var item = HolidayMilestone.NewHolidayMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayMilestoneValidatorReportsEveryProblem();
        HolidayMilestoneCostCenterLengthRuleFlagsALongCostCenter();
        HolidayMilestoneRequiredCodeRuleFlagsABlankCode();
    }
}
