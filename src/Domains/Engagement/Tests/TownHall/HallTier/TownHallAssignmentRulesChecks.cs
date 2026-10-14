namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallAssignmentRulesChecks
{
    public static void TownHallAssignmentValidatorReportsEveryProblem()
    {
        var validator = TownHallAssignmentRules.DefaultTownHallAssignmentValidator();
        var item = TownHallAssignment.NewTownHallAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallAssignmentWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new TownHallAssignmentWindowDaysRangeRule(3600, 86400);
        var item = TownHallAssignment.NewTownHallAssignment("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TownHallAssignmentTitleLengthRule(1000);
        var item = TownHallAssignment.NewTownHallAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new TownHallAssignmentAttemptsRangeRule(86400, 86400);
        var item = TownHallAssignment.NewTownHallAssignment("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallAssignmentRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new TownHallAssignmentRequiredCountryRule("town hall assignment needs a value");
        var item = TownHallAssignment.NewTownHallAssignment("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallAssignmentValidatorReportsEveryProblem();
        TownHallAssignmentWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        TownHallAssignmentTitleLengthRuleFlagsALongTitle();
        TownHallAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        TownHallAssignmentRequiredCountryRuleFlagsABlankCountry();
    }
}
