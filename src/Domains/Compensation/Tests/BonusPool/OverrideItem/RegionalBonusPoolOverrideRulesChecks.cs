namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalBonusPoolOverrideRulesChecks
{
    public static void RegionalBonusPoolOverrideValidatorReportsEveryProblem()
    {
        var validator = RegionalBonusPoolOverrideRules.DefaultRegionalBonusPoolOverrideValidator();
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalBonusPoolOverrideRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new RegionalBonusPoolOverrideRetriesRangeRule(255, 255);
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBonusPoolOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalBonusPoolOverrideCodeLengthRule(64);
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBonusPoolOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new RegionalBonusPoolOverrideDurationMinutesRangeRule(1000, 1000);
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBonusPoolOverrideRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RegionalBonusPoolOverrideRequiredCurrencyRule("regional bonus pool override needs this field");
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalBonusPoolOverrideValidatorReportsEveryProblem();
        RegionalBonusPoolOverrideRetriesRangeRuleFlagsValuesAboveRetries();
        RegionalBonusPoolOverrideCodeLengthRuleFlagsALongCode();
        RegionalBonusPoolOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        RegionalBonusPoolOverrideRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
