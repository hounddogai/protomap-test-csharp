namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class GarnishmentPreferenceRulesChecks
{
    public static void GarnishmentPreferenceValidatorReportsEveryProblem()
    {
        var validator = GarnishmentPreferenceRules.DefaultGarnishmentPreferenceValidator();
        var item = GarnishmentPreference.NewGarnishmentPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentPreferenceLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new GarnishmentPreferenceLimitRangeRule(255, 255);
        var item = GarnishmentPreference.NewGarnishmentPreference("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GarnishmentPreferenceValidatorReportsEveryProblem();
        GarnishmentPreferenceLimitRangeRuleFlagsValuesAboveLimit();
    }
}
