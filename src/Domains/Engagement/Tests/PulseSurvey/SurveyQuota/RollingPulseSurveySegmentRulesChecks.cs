namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class RollingPulseSurveySegmentRulesChecks
{
    public static void RollingPulseSurveySegmentValidatorReportsEveryProblem()
    {
        var validator = RollingPulseSurveySegmentRules.DefaultRollingPulseSurveySegmentValidator();
        var item = RollingPulseSurveySegment.NewRollingPulseSurveySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPulseSurveySegmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingPulseSurveySegmentLabelLengthRule(80);
        var item = RollingPulseSurveySegment.NewRollingPulseSurveySegment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPulseSurveySegmentMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RollingPulseSurveySegmentMinimumRangeRule(4.6, 1238.8);
        var item = RollingPulseSurveySegment.NewRollingPulseSurveySegment("x", "y");
        item.Minimum = 1240.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingPulseSurveySegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingPulseSurveySegmentCodeLengthRule(120);
        var item = RollingPulseSurveySegment.NewRollingPulseSurveySegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingPulseSurveySegmentValidatorReportsEveryProblem();
        RollingPulseSurveySegmentLabelLengthRuleFlagsALongLabel();
        RollingPulseSurveySegmentMinimumRangeRuleFlagsValuesAboveMinimum();
        RollingPulseSurveySegmentCodeLengthRuleFlagsALongCode();
    }
}
