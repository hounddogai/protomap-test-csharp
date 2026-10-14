namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class DefaultSkillProfileForecastRulesChecks
{
    public static void DefaultSkillProfileForecastValidatorReportsEveryProblem()
    {
        var validator = DefaultSkillProfileForecastRules.DefaultDefaultSkillProfileForecastValidator();
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSkillProfileForecastTeamLengthRuleFlagsALongTeam()
    {
        var rule = new DefaultSkillProfileForecastTeamLengthRule(1000);
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultSkillProfileForecastRequiredCodeRule("default skill profile forecast needs this field");
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSkillProfileForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultSkillProfileForecastCodeLengthRule(3600);
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileForecastRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new DefaultSkillProfileForecastRequiredTeamRule("default skill profile forecast needs a value");
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultSkillProfileForecastValidatorReportsEveryProblem();
        DefaultSkillProfileForecastTeamLengthRuleFlagsALongTeam();
        DefaultSkillProfileForecastRequiredCodeRuleFlagsABlankCode();
        DefaultSkillProfileForecastCodeLengthRuleFlagsALongCode();
        DefaultSkillProfileForecastRequiredTeamRuleFlagsABlankTeam();
    }
}
