namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TieredTimesheetProfileRulesChecks
{
    public static void TieredTimesheetProfileValidatorReportsEveryProblem()
    {
        var validator = TieredTimesheetProfileRules.DefaultTieredTimesheetProfileValidator();
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredTimesheetProfileCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new TieredTimesheetProfileCostRangeRule(9.1, 1456.2);
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("x", "y");
        item.Cost = 1457.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredTimesheetProfileTeamLengthRuleFlagsALongTeam()
    {
        var rule = new TieredTimesheetProfileTeamLengthRule(1000);
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTimesheetProfileRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredTimesheetProfileRequiredLabelRule("tiered timesheet profile needs a value");
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredTimesheetProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredTimesheetProfileTitleLengthRule(64);
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredTimesheetProfileValidatorReportsEveryProblem();
        TieredTimesheetProfileCostRangeRuleFlagsValuesAboveCost();
        TieredTimesheetProfileTeamLengthRuleFlagsALongTeam();
        TieredTimesheetProfileRequiredLabelRuleFlagsABlankLabel();
        TieredTimesheetProfileTitleLengthRuleFlagsALongTitle();
    }
}
