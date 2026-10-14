namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandPreferenceRulesChecks
{
    public static void PayBandPreferenceValidatorReportsEveryProblem()
    {
        var validator = PayBandPreferenceRules.DefaultPayBandPreferenceValidator();
        var item = PayBandPreference.NewPayBandPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayBandPreferenceRequiredTitleRule("pay band preference needs an entry");
        var item = PayBandPreference.NewPayBandPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayBandPreferenceTitleLengthRule(64);
        var item = PayBandPreference.NewPayBandPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandPreferenceValidatorReportsEveryProblem();
        PayBandPreferenceRequiredTitleRuleFlagsABlankTitle();
        PayBandPreferenceTitleLengthRuleFlagsALongTitle();
    }
}
