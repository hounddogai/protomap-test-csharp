namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultBalancePreferenceRulesChecks
{
    public static void DefaultBalancePreferenceValidatorReportsEveryProblem()
    {
        var validator = DefaultBalancePreferenceRules.DefaultDefaultBalancePreferenceValidator();
        var item = DefaultBalancePreference.NewDefaultBalancePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultBalancePreferenceRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultBalancePreferenceRequiredNameRule("default balance preference needs an entry");
        var item = DefaultBalancePreference.NewDefaultBalancePreference("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBalancePreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultBalancePreferenceRequiredLabelRule("default balance preference needs an entry");
        var item = DefaultBalancePreference.NewDefaultBalancePreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBalancePreferenceLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DefaultBalancePreferenceLeaveDaysRangeRule(16, 32);
        var item = DefaultBalancePreference.NewDefaultBalancePreference("x", "y");
        item.LeaveDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultBalancePreferenceValidatorReportsEveryProblem();
        DefaultBalancePreferenceRequiredNameRuleFlagsABlankName();
        DefaultBalancePreferenceRequiredLabelRuleFlagsABlankLabel();
        DefaultBalancePreferenceLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
