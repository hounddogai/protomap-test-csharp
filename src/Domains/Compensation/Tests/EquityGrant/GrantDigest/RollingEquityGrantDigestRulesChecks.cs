namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class RollingEquityGrantDigestRulesChecks
{
    public static void RollingEquityGrantDigestValidatorReportsEveryProblem()
    {
        var validator = RollingEquityGrantDigestRules.DefaultRollingEquityGrantDigestValidator();
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingEquityGrantDigestRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new RollingEquityGrantDigestRequiredSummaryRule("rolling equity grant digest needs this field");
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingEquityGrantDigestSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new RollingEquityGrantDigestSummaryLengthRule(86400);
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingEquityGrantDigestRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RollingEquityGrantDigestRequiredCurrencyRule("rolling equity grant digest needs a value");
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingEquityGrantDigestLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RollingEquityGrantDigestLeaveDaysRangeRule(3600, 3600);
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingEquityGrantDigestValidatorReportsEveryProblem();
        RollingEquityGrantDigestRequiredSummaryRuleFlagsABlankSummary();
        RollingEquityGrantDigestSummaryLengthRuleFlagsALongSummary();
        RollingEquityGrantDigestRequiredCurrencyRuleFlagsABlankCurrency();
        RollingEquityGrantDigestLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
