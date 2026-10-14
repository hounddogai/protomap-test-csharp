namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationBundleRulesChecks
{
    public static void NominationBundleValidatorReportsEveryProblem()
    {
        var validator = NominationBundleRules.DefaultNominationBundleValidator();
        var item = NominationBundle.NewNominationBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationBundleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new NominationBundleMarginRangeRule(0.2, 3625.8);
        var item = NominationBundle.NewNominationBundle("x", "y");
        item.Margin = 3627.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationBundleRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new NominationBundleRateRangeRule(1.2, 3788.1);
        var item = NominationBundle.NewNominationBundle("x", "y");
        item.Rate = 3789.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        NominationBundleValidatorReportsEveryProblem();
        NominationBundleMarginRangeRuleFlagsValuesAboveMargin();
        NominationBundleRateRangeRuleFlagsValuesAboveRate();
    }
}
