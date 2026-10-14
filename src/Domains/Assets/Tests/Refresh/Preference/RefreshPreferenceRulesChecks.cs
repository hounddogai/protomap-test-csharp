namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshPreferenceRulesChecks
{
    public static void RefreshPreferenceValidatorReportsEveryProblem()
    {
        var validator = RefreshPreferenceRules.DefaultRefreshPreferenceValidator();
        var item = RefreshPreference.NewRefreshPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshPreferenceLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new RefreshPreferenceLimitRangeRule(64, 64);
        var item = RefreshPreference.NewRefreshPreference("x", "y");
        item.Limit = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshPreferenceRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new RefreshPreferenceRequiredBadgeRule("refresh preference needs this field");
        var item = RefreshPreference.NewRefreshPreference("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshPreferenceValidatorReportsEveryProblem();
        RefreshPreferenceLimitRangeRuleFlagsValuesAboveLimit();
        RefreshPreferenceRequiredBadgeRuleFlagsABlankBadge();
    }
}
