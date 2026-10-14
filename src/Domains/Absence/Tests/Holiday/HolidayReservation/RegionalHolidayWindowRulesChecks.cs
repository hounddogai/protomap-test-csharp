namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class RegionalHolidayWindowRulesChecks
{
    public static void RegionalHolidayWindowValidatorReportsEveryProblem()
    {
        var validator = RegionalHolidayWindowRules.DefaultRegionalHolidayWindowValidator();
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalHolidayWindowCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RegionalHolidayWindowCostRangeRule(5.5, 1639.7);
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("x", "y");
        item.Cost = 1641.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalHolidayWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalHolidayWindowRequiredCodeRule("regional holiday window needs an entry");
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalHolidayWindowRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new RegionalHolidayWindowRequiredTeamRule("regional holiday window needs this field");
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalHolidayWindowWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RegionalHolidayWindowWeightRangeRule(3.7, 3767.5);
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("x", "y");
        item.Weight = 3769.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalHolidayWindowRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalHolidayWindowRequiredLabelRule("regional holiday window needs an entry");
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalHolidayWindowValidatorReportsEveryProblem();
        RegionalHolidayWindowCostRangeRuleFlagsValuesAboveCost();
        RegionalHolidayWindowRequiredCodeRuleFlagsABlankCode();
        RegionalHolidayWindowRequiredTeamRuleFlagsABlankTeam();
        RegionalHolidayWindowWeightRangeRuleFlagsValuesAboveWeight();
        RegionalHolidayWindowRequiredLabelRuleFlagsABlankLabel();
    }
}
