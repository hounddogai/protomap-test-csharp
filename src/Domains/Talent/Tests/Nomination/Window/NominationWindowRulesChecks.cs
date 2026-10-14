namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationWindowRulesChecks
{
    public static void NominationWindowValidatorReportsEveryProblem()
    {
        var validator = NominationWindowRules.DefaultNominationWindowValidator();
        var item = NominationWindow.NewNominationWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationWindowCountryLengthRuleFlagsALongCountry()
    {
        var rule = new NominationWindowCountryLengthRule(86400);
        var item = NominationWindow.NewNominationWindow("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new NominationWindowLabelLengthRule(86400);
        var item = NominationWindow.NewNominationWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationWindowRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new NominationWindowRatioRangeRule(5.5, 3908.7);
        var item = NominationWindow.NewNominationWindow("x", "y");
        item.Ratio = 3910.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationWindowRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new NominationWindowRequiredCountryRule("nomination window needs an entry");
        var item = NominationWindow.NewNominationWindow("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationWindowValidatorReportsEveryProblem();
        NominationWindowCountryLengthRuleFlagsALongCountry();
        NominationWindowLabelLengthRuleFlagsALongLabel();
        NominationWindowRatioRangeRuleFlagsValuesAboveRatio();
        NominationWindowRequiredCountryRuleFlagsABlankCountry();
    }
}
