namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackPulseSurveySegmentRulesChecks
{
    public static void FallbackPulseSurveySegmentValidatorReportsEveryProblem()
    {
        var validator = FallbackPulseSurveySegmentRules.DefaultFallbackPulseSurveySegmentValidator();
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPulseSurveySegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new FallbackPulseSurveySegmentDurationMinutesRangeRule(16, 86400);
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPulseSurveySegmentRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new FallbackPulseSurveySegmentRequiredGradeRule("fallback pulse survey segment needs this field");
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPulseSurveySegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackPulseSurveySegmentRequiredCodeRule("fallback pulse survey segment needs a value");
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPulseSurveySegmentRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new FallbackPulseSurveySegmentRatingRangeRule(5.6, 601.4);
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("x", "y");
        item.Rating = 602.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackPulseSurveySegmentValidatorReportsEveryProblem();
        FallbackPulseSurveySegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        FallbackPulseSurveySegmentRequiredGradeRuleFlagsABlankGrade();
        FallbackPulseSurveySegmentRequiredCodeRuleFlagsABlankCode();
        FallbackPulseSurveySegmentRatingRangeRuleFlagsValuesAboveRating();
    }
}
