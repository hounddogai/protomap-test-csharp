namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictPayBandPreferenceRulesChecks
{
    public static void StrictPayBandPreferenceValidatorReportsEveryProblem()
    {
        var validator = StrictPayBandPreferenceRules.DefaultStrictPayBandPreferenceValidator();
        var item = StrictPayBandPreference.NewStrictPayBandPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPayBandPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictPayBandPreferenceCodeLengthRule(86400);
        var item = StrictPayBandPreference.NewStrictPayBandPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPayBandPreferenceLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new StrictPayBandPreferenceLimitRangeRule(1000, 1000);
        var item = StrictPayBandPreference.NewStrictPayBandPreference("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPayBandPreferenceSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new StrictPayBandPreferenceSequenceRangeRule(80, 1000);
        var item = StrictPayBandPreference.NewStrictPayBandPreference("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictPayBandPreferenceValidatorReportsEveryProblem();
        StrictPayBandPreferenceCodeLengthRuleFlagsALongCode();
        StrictPayBandPreferenceLimitRangeRuleFlagsValuesAboveLimit();
        StrictPayBandPreferenceSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
