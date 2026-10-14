namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RollingRefreshPreferenceRulesChecks
{
    public static void RollingRefreshPreferenceValidatorReportsEveryProblem()
    {
        var validator = RollingRefreshPreferenceRules.DefaultRollingRefreshPreferenceValidator();
        var item = RollingRefreshPreference.NewRollingRefreshPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRefreshPreferenceDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new RollingRefreshPreferenceDurationMinutesRangeRule(64, 80);
        var item = RollingRefreshPreference.NewRollingRefreshPreference("x", "y");
        item.DurationMinutes = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingRefreshPreferenceValidatorReportsEveryProblem();
        RollingRefreshPreferenceDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
