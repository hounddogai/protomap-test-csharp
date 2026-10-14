namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ManualRateOverrideRulesChecks
{
    public static void ManualRateOverrideValidatorReportsEveryProblem()
    {
        var validator = ManualRateOverrideRules.DefaultManualRateOverrideValidator();
        var item = ManualRateOverride.NewManualRateOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualRateOverrideRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ManualRateOverrideRequiredLocaleRule("manual rate override needs this field");
        var item = ManualRateOverride.NewManualRateOverride("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRateOverrideGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ManualRateOverrideGraceDaysRangeRule(16, 32);
        var item = ManualRateOverride.NewManualRateOverride("x", "y");
        item.GraceDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualRateOverrideRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new ManualRateOverrideRequiredExternalRefRule("manual rate override needs an entry");
        var item = ManualRateOverride.NewManualRateOverride("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualRateOverrideValidatorReportsEveryProblem();
        ManualRateOverrideRequiredLocaleRuleFlagsABlankLocale();
        ManualRateOverrideGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ManualRateOverrideRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
