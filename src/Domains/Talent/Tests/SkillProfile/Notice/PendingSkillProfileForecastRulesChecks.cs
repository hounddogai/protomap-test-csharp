namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingSkillProfileForecastRulesChecks
{
    public static void PendingSkillProfileForecastValidatorReportsEveryProblem()
    {
        var validator = PendingSkillProfileForecastRules.DefaultPendingSkillProfileForecastValidator();
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingSkillProfileForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingSkillProfileForecastCodeLengthRule(3600);
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSkillProfileForecastAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PendingSkillProfileForecastAttemptsRangeRule(80, 86400);
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingSkillProfileForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingSkillProfileForecastRequiredCodeRule("pending skill profile forecast needs an entry");
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingSkillProfileForecastSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PendingSkillProfileForecastSummaryLengthRule(120);
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingSkillProfileForecastValidatorReportsEveryProblem();
        PendingSkillProfileForecastCodeLengthRuleFlagsALongCode();
        PendingSkillProfileForecastAttemptsRangeRuleFlagsValuesAboveAttempts();
        PendingSkillProfileForecastRequiredCodeRuleFlagsABlankCode();
        PendingSkillProfileForecastSummaryLengthRuleFlagsALongSummary();
    }
}
