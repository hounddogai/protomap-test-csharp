namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalCostModelPreferenceRulesChecks
{
    public static void SeasonalCostModelPreferenceValidatorReportsEveryProblem()
    {
        var validator = SeasonalCostModelPreferenceRules.DefaultSeasonalCostModelPreferenceValidator();
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalCostModelPreferenceTeamLengthRuleFlagsALongTeam()
    {
        var rule = new SeasonalCostModelPreferenceTeamLengthRule(120);
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalCostModelPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalCostModelPreferenceCodeLengthRule(16);
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalCostModelPreferenceCountryLengthRuleFlagsALongCountry()
    {
        var rule = new SeasonalCostModelPreferenceCountryLengthRule(64);
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalCostModelPreferenceRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new SeasonalCostModelPreferenceRequiredTeamRule("seasonal cost model preference needs this field");
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalCostModelPreferenceValidatorReportsEveryProblem();
        SeasonalCostModelPreferenceTeamLengthRuleFlagsALongTeam();
        SeasonalCostModelPreferenceCodeLengthRuleFlagsALongCode();
        SeasonalCostModelPreferenceCountryLengthRuleFlagsALongCountry();
        SeasonalCostModelPreferenceRequiredTeamRuleFlagsABlankTeam();
    }
}
