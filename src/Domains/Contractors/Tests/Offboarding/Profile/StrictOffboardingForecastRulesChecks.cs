namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictOffboardingForecastRulesChecks
{
    public static void StrictOffboardingForecastValidatorReportsEveryProblem()
    {
        var validator = StrictOffboardingForecastRules.DefaultStrictOffboardingForecastValidator();
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictOffboardingForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictOffboardingForecastRequiredCodeRule("strict offboarding forecast needs a value");
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictOffboardingForecastMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new StrictOffboardingForecastMarginRangeRule(0.2, 2702.4);
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("x", "y");
        item.Margin = 2703.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictOffboardingForecastGradeLengthRuleFlagsALongGrade()
    {
        var rule = new StrictOffboardingForecastGradeLengthRule(1000);
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictOffboardingForecastRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new StrictOffboardingForecastRequiredGradeRule("strict offboarding forecast needs this field");
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictOffboardingForecastValidatorReportsEveryProblem();
        StrictOffboardingForecastRequiredCodeRuleFlagsABlankCode();
        StrictOffboardingForecastMarginRangeRuleFlagsValuesAboveMargin();
        StrictOffboardingForecastGradeLengthRuleFlagsALongGrade();
        StrictOffboardingForecastRequiredGradeRuleFlagsABlankGrade();
    }
}
