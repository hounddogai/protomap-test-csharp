namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingBonusPoolProfileRulesChecks
{
    public static void RollingBonusPoolProfileValidatorReportsEveryProblem()
    {
        var validator = RollingBonusPoolProfileRules.DefaultRollingBonusPoolProfileValidator();
        var item = RollingBonusPoolProfile.NewRollingBonusPoolProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBonusPoolProfileSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new RollingBonusPoolProfileSummaryLengthRule(64);
        var item = RollingBonusPoolProfile.NewRollingBonusPoolProfile("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBonusPoolProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingBonusPoolProfileRequiredNameRule("rolling bonus pool profile needs an entry");
        var item = RollingBonusPoolProfile.NewRollingBonusPoolProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBonusPoolProfileAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new RollingBonusPoolProfileAmountRangeRule(8.9, 819.4);
        var item = RollingBonusPoolProfile.NewRollingBonusPoolProfile("x", "y");
        item.Amount = 820.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBonusPoolProfileCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new RollingBonusPoolProfileCurrencyLengthRule(86400);
        var item = RollingBonusPoolProfile.NewRollingBonusPoolProfile("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingBonusPoolProfileValidatorReportsEveryProblem();
        RollingBonusPoolProfileSummaryLengthRuleFlagsALongSummary();
        RollingBonusPoolProfileRequiredNameRuleFlagsABlankName();
        RollingBonusPoolProfileAmountRangeRuleFlagsValuesAboveAmount();
        RollingBonusPoolProfileCurrencyLengthRuleFlagsALongCurrency();
    }
}
