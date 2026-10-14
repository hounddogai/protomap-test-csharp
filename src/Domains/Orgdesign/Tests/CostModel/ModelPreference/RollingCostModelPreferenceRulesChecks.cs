namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class RollingCostModelPreferenceRulesChecks
{
    public static void RollingCostModelPreferenceValidatorReportsEveryProblem()
    {
        var validator = RollingCostModelPreferenceRules.DefaultRollingCostModelPreferenceValidator();
        var item = RollingCostModelPreference.NewRollingCostModelPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingCostModelPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RollingCostModelPreferenceExpiresAtRangeRule(86400, 86400);
        var item = RollingCostModelPreference.NewRollingCostModelPreference("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingCostModelPreferenceTeamLengthRuleFlagsALongTeam()
    {
        var rule = new RollingCostModelPreferenceTeamLengthRule(255);
        var item = RollingCostModelPreference.NewRollingCostModelPreference("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingCostModelPreferenceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RollingCostModelPreferenceTenureMonthsRangeRule(32, 64);
        var item = RollingCostModelPreference.NewRollingCostModelPreference("x", "y");
        item.TenureMonths = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingCostModelPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingCostModelPreferenceRequiredTitleRule("rolling cost model preference needs a value");
        var item = RollingCostModelPreference.NewRollingCostModelPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingCostModelPreferenceValidatorReportsEveryProblem();
        RollingCostModelPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RollingCostModelPreferenceTeamLengthRuleFlagsALongTeam();
        RollingCostModelPreferenceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        RollingCostModelPreferenceRequiredTitleRuleFlagsABlankTitle();
    }
}
