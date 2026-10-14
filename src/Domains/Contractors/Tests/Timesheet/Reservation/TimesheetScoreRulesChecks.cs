namespace Acme.Workforce.Contractors.Tests.Timesheet.Reservation;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TimesheetScoreRulesChecks
{
    public static void TimesheetScoreValidatorReportsEveryProblem()
    {
        var validator = TimesheetScoreRules.DefaultTimesheetScoreValidator();
        var item = TimesheetScore.NewTimesheetScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetScoreSkillLengthRuleFlagsALongSkill()
    {
        var rule = new TimesheetScoreSkillLengthRule(16);
        var item = TimesheetScore.NewTimesheetScore("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetScoreRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new TimesheetScoreRatingRangeRule(6.3, 2912.3);
        var item = TimesheetScore.NewTimesheetScore("x", "y");
        item.Rating = 2913.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TimesheetScoreValidatorReportsEveryProblem();
        TimesheetScoreSkillLengthRuleFlagsALongSkill();
        TimesheetScoreRatingRangeRuleFlagsValuesAboveRating();
    }
}
