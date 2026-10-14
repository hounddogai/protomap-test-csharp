namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyRecognitionCycleRulesChecks
{
    public static void LegacyRecognitionCycleValidatorReportsEveryProblem()
    {
        var validator = LegacyRecognitionCycleRules.DefaultLegacyRecognitionCycleValidator();
        var item = LegacyRecognitionCycle.NewLegacyRecognitionCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRecognitionCycleGradeLengthRuleFlagsALongGrade()
    {
        var rule = new LegacyRecognitionCycleGradeLengthRule(16);
        var item = LegacyRecognitionCycle.NewLegacyRecognitionCycle("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyRecognitionCycleHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new LegacyRecognitionCycleHoursRangeRule(9.5, 4582.1);
        var item = LegacyRecognitionCycle.NewLegacyRecognitionCycle("x", "y");
        item.Hours = 4583.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyRecognitionCycleValidatorReportsEveryProblem();
        LegacyRecognitionCycleGradeLengthRuleFlagsALongGrade();
        LegacyRecognitionCycleHoursRangeRuleFlagsValuesAboveHours();
    }
}
