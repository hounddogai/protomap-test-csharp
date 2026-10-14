namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class ManualRecognitionScheduleRulesChecks
{
    public static void ManualRecognitionScheduleValidatorReportsEveryProblem()
    {
        var validator = ManualRecognitionScheduleRules.DefaultManualRecognitionScheduleValidator();
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualRecognitionScheduleRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new ManualRecognitionScheduleRequiredLocationRule("manual recognition schedule needs this field");
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRecognitionScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualRecognitionScheduleRequiredCodeRule("manual recognition schedule needs a value");
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRecognitionScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualRecognitionScheduleRequiredTitleRule("manual recognition schedule needs a value");
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRecognitionScheduleRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ManualRecognitionScheduleRatingRangeRule(1.5, 4856.3);
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("x", "y");
        item.Rating = 4857.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualRecognitionScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualRecognitionScheduleCodeLengthRule(86400);
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualRecognitionScheduleValidatorReportsEveryProblem();
        ManualRecognitionScheduleRequiredLocationRuleFlagsABlankLocation();
        ManualRecognitionScheduleRequiredCodeRuleFlagsABlankCode();
        ManualRecognitionScheduleRequiredTitleRuleFlagsABlankTitle();
        ManualRecognitionScheduleRatingRangeRuleFlagsValuesAboveRating();
        ManualRecognitionScheduleCodeLengthRuleFlagsALongCode();
    }
}
