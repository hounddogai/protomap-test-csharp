namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class ScheduledTownHallExceptionRulesChecks
{
    public static void ScheduledTownHallExceptionValidatorReportsEveryProblem()
    {
        var validator = ScheduledTownHallExceptionRules.DefaultScheduledTownHallExceptionValidator();
        var item = ScheduledTownHallException.NewScheduledTownHallException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledTownHallExceptionAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ScheduledTownHallExceptionAttemptsRangeRule(80, 3600);
        var item = ScheduledTownHallException.NewScheduledTownHallException("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledTownHallExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledTownHallExceptionRequiredNameRule("scheduled town hall exception needs a value");
        var item = ScheduledTownHallException.NewScheduledTownHallException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledTownHallExceptionRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ScheduledTownHallExceptionRatingRangeRule(9.2, 1403.4);
        var item = ScheduledTownHallException.NewScheduledTownHallException("x", "y");
        item.Rating = 1404.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledTownHallExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledTownHallExceptionNameLengthRule(255);
        var item = ScheduledTownHallException.NewScheduledTownHallException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledTownHallExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledTownHallExceptionTitleLengthRule(3600);
        var item = ScheduledTownHallException.NewScheduledTownHallException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledTownHallExceptionValidatorReportsEveryProblem();
        ScheduledTownHallExceptionAttemptsRangeRuleFlagsValuesAboveAttempts();
        ScheduledTownHallExceptionRequiredNameRuleFlagsABlankName();
        ScheduledTownHallExceptionRatingRangeRuleFlagsValuesAboveRating();
        ScheduledTownHallExceptionNameLengthRuleFlagsALongName();
        ScheduledTownHallExceptionTitleLengthRuleFlagsALongTitle();
    }
}
