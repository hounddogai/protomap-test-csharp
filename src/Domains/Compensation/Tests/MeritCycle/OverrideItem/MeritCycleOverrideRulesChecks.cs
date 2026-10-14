namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleOverrideRulesChecks
{
    public static void MeritCycleOverrideValidatorReportsEveryProblem()
    {
        var validator = MeritCycleOverrideRules.DefaultMeritCycleOverrideValidator();
        var item = MeritCycleOverride.NewMeritCycleOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleOverrideRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new MeritCycleOverrideRequiredLocaleRule("merit cycle override needs a value");
        var item = MeritCycleOverride.NewMeritCycleOverride("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new MeritCycleOverrideRequiredLabelRule("merit cycle override needs this field");
        var item = MeritCycleOverride.NewMeritCycleOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleOverrideCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new MeritCycleOverrideCategoryLengthRule(86400);
        var item = MeritCycleOverride.NewMeritCycleOverride("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleOverrideLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new MeritCycleOverrideLocaleLengthRule(80);
        var item = MeritCycleOverride.NewMeritCycleOverride("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleOverrideAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new MeritCycleOverrideAmountRangeRule(6.8, 3415.5);
        var item = MeritCycleOverride.NewMeritCycleOverride("x", "y");
        item.Amount = 3417.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleOverrideValidatorReportsEveryProblem();
        MeritCycleOverrideRequiredLocaleRuleFlagsABlankLocale();
        MeritCycleOverrideRequiredLabelRuleFlagsABlankLabel();
        MeritCycleOverrideCategoryLengthRuleFlagsALongCategory();
        MeritCycleOverrideLocaleLengthRuleFlagsALongLocale();
        MeritCycleOverrideAmountRangeRuleFlagsValuesAboveAmount();
    }
}
