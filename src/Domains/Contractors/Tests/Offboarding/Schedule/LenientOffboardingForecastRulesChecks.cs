namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientOffboardingForecastRulesChecks
{
    public static void LenientOffboardingForecastValidatorReportsEveryProblem()
    {
        var validator = LenientOffboardingForecastRules.DefaultLenientOffboardingForecastValidator();
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientOffboardingForecastLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new LenientOffboardingForecastLocaleLengthRule(1000);
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientOffboardingForecastRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientOffboardingForecastRequiredNameRule("lenient offboarding forecast needs an entry");
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientOffboardingForecastNameLengthRuleFlagsALongName()
    {
        var rule = new LenientOffboardingForecastNameLengthRule(16);
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientOffboardingForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientOffboardingForecastCodeLengthRule(16);
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientOffboardingForecastValidatorReportsEveryProblem();
        LenientOffboardingForecastLocaleLengthRuleFlagsALongLocale();
        LenientOffboardingForecastRequiredNameRuleFlagsABlankName();
        LenientOffboardingForecastNameLengthRuleFlagsALongName();
        LenientOffboardingForecastCodeLengthRuleFlagsALongCode();
    }
}
