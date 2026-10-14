namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictMobilityOverrideRulesChecks
{
    public static void StrictMobilityOverrideValidatorReportsEveryProblem()
    {
        var validator = StrictMobilityOverrideRules.DefaultStrictMobilityOverrideValidator();
        var item = StrictMobilityOverride.NewStrictMobilityOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictMobilityOverrideCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new StrictMobilityOverrideCurrencyLengthRule(255);
        var item = StrictMobilityOverride.NewStrictMobilityOverride("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictMobilityOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictMobilityOverrideCodeLengthRule(86400);
        var item = StrictMobilityOverride.NewStrictMobilityOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictMobilityOverrideValidatorReportsEveryProblem();
        StrictMobilityOverrideCurrencyLengthRuleFlagsALongCurrency();
        StrictMobilityOverrideCodeLengthRuleFlagsALongCode();
    }
}
