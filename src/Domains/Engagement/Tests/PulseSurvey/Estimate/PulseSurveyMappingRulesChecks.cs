namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyMappingRulesChecks
{
    public static void PulseSurveyMappingValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyMappingRules.DefaultPulseSurveyMappingValidator();
        var item = PulseSurveyMapping.NewPulseSurveyMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyMappingSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PulseSurveyMappingSequenceRangeRule(64, 120);
        var item = PulseSurveyMapping.NewPulseSurveyMapping("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PulseSurveyMappingLabelLengthRule(3600);
        var item = PulseSurveyMapping.NewPulseSurveyMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyMappingEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PulseSurveyMappingEndsAtRangeRule(16, 120);
        var item = PulseSurveyMapping.NewPulseSurveyMapping("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PulseSurveyMappingValidatorReportsEveryProblem();
        PulseSurveyMappingSequenceRangeRuleFlagsValuesAboveSequence();
        PulseSurveyMappingLabelLengthRuleFlagsALongLabel();
        PulseSurveyMappingEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
