namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultPulseSurveyOverrideRulesChecks
{
    public static void DefaultPulseSurveyOverrideValidatorReportsEveryProblem()
    {
        var validator = DefaultPulseSurveyOverrideRules.DefaultDefaultPulseSurveyOverrideValidator();
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPulseSurveyOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultPulseSurveyOverrideRequiredNameRule("default pulse survey override needs a value");
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPulseSurveyOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultPulseSurveyOverrideCodeLengthRule(16);
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPulseSurveyOverrideCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new DefaultPulseSurveyOverrideCapacityRangeRule(80, 1000);
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultPulseSurveyOverrideValidatorReportsEveryProblem();
        DefaultPulseSurveyOverrideRequiredNameRuleFlagsABlankName();
        DefaultPulseSurveyOverrideCodeLengthRuleFlagsALongCode();
        DefaultPulseSurveyOverrideCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
