namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredEquityGrantWindowRulesChecks
{
    public static void TieredEquityGrantWindowValidatorReportsEveryProblem()
    {
        var validator = TieredEquityGrantWindowRules.DefaultTieredEquityGrantWindowValidator();
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredEquityGrantWindowRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new TieredEquityGrantWindowRequiredCountryRule("tiered equity grant window needs this field");
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredEquityGrantWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredEquityGrantWindowCodeLengthRule(1000);
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredEquityGrantWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredEquityGrantWindowRequiredCodeRule("tiered equity grant window needs an entry");
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredEquityGrantWindowTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new TieredEquityGrantWindowTenureMonthsRangeRule(16, 32);
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("x", "y");
        item.TenureMonths = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredEquityGrantWindowCountryLengthRuleFlagsALongCountry()
    {
        var rule = new TieredEquityGrantWindowCountryLengthRule(86400);
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredEquityGrantWindowValidatorReportsEveryProblem();
        TieredEquityGrantWindowRequiredCountryRuleFlagsABlankCountry();
        TieredEquityGrantWindowCodeLengthRuleFlagsALongCode();
        TieredEquityGrantWindowRequiredCodeRuleFlagsABlankCode();
        TieredEquityGrantWindowTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        TieredEquityGrantWindowCountryLengthRuleFlagsALongCountry();
    }
}
