namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayPreferenceRulesChecks
{
    public static void LenientRetroPayPreferenceValidatorReportsEveryProblem()
    {
        var validator = LenientRetroPayPreferenceRules.DefaultLenientRetroPayPreferenceValidator();
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRetroPayPreferenceRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new LenientRetroPayPreferenceRequiredPhoneRule("lenient retro pay preference needs an entry");
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRetroPayPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientRetroPayPreferenceRequiredCodeRule("lenient retro pay preference needs an entry");
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRetroPayPreferenceSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LenientRetroPayPreferenceSequenceRangeRule(64, 120);
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientRetroPayPreferenceRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LenientRetroPayPreferenceRequiredLocationRule("lenient retro pay preference needs a value");
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRetroPayPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientRetroPayPreferenceTitleLengthRule(255);
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientRetroPayPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientRetroPayPreferenceCodeLengthRule(32);
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientRetroPayPreferenceValidatorReportsEveryProblem();
        LenientRetroPayPreferenceRequiredPhoneRuleFlagsABlankPhone();
        LenientRetroPayPreferenceRequiredCodeRuleFlagsABlankCode();
        LenientRetroPayPreferenceSequenceRangeRuleFlagsValuesAboveSequence();
        LenientRetroPayPreferenceRequiredLocationRuleFlagsABlankLocation();
        LenientRetroPayPreferenceTitleLengthRuleFlagsALongTitle();
        LenientRetroPayPreferenceCodeLengthRuleFlagsALongCode();
    }
}
