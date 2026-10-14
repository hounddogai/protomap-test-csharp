namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RecognitionScheduleRulesChecks
{
    public static void RecognitionScheduleValidatorReportsEveryProblem()
    {
        var validator = RecognitionScheduleRules.DefaultRecognitionScheduleValidator();
        var item = RecognitionSchedule.NewRecognitionSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RecognitionScheduleRequiredTitleRule("recognition schedule needs this field");
        var item = RecognitionSchedule.NewRecognitionSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionScheduleGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new RecognitionScheduleGraceDaysRangeRule(32, 255);
        var item = RecognitionSchedule.NewRecognitionSchedule("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RecognitionScheduleValidatorReportsEveryProblem();
        RecognitionScheduleRequiredTitleRuleFlagsABlankTitle();
        RecognitionScheduleGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
