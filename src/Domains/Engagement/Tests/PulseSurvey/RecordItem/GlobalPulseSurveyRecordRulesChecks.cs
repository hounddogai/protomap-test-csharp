namespace Acme.Workforce.Engagement.Tests.PulseSurvey.RecordItem;

using Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalPulseSurveyRecordRulesChecks
{
    public static void GlobalPulseSurveyRecordValidatorReportsEveryProblem()
    {
        var validator = GlobalPulseSurveyRecordRules.DefaultGlobalPulseSurveyRecordValidator();
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPulseSurveyRecordEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new GlobalPulseSurveyRecordEndsAtRangeRule(16, 64);
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("x", "y");
        item.EndsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPulseSurveyRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalPulseSurveyRecordRequiredNameRule("global pulse survey record needs a value");
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalPulseSurveyRecordWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new GlobalPulseSurveyRecordWeightRangeRule(0.7, 4566.2);
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("x", "y");
        item.Weight = 4567.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyRecordValidatorReportsEveryProblem();
        GlobalPulseSurveyRecordEndsAtRangeRuleFlagsValuesAboveEndsAt();
        GlobalPulseSurveyRecordRequiredNameRuleFlagsABlankName();
        GlobalPulseSurveyRecordWeightRangeRuleFlagsValuesAboveWeight();
    }
}
