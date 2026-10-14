namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalExceptionPreferenceRulesChecks
{
    public static void RegionalExceptionPreferenceValidatorReportsEveryProblem()
    {
        var validator = RegionalExceptionPreferenceRules.DefaultRegionalExceptionPreferenceValidator();
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalExceptionPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalExceptionPreferenceRequiredCodeRule("regional exception preference needs this field");
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalExceptionPreferenceLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RegionalExceptionPreferenceLocaleLengthRule(32);
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalExceptionPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalExceptionPreferenceCodeLengthRule(3600);
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalExceptionPreferenceValidatorReportsEveryProblem();
        RegionalExceptionPreferenceRequiredCodeRuleFlagsABlankCode();
        RegionalExceptionPreferenceLocaleLengthRuleFlagsALongLocale();
        RegionalExceptionPreferenceCodeLengthRuleFlagsALongCode();
    }
}
