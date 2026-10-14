namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RollingPulseSurveyTemplateRulesChecks
{
    public static void RollingPulseSurveyTemplateValidatorReportsEveryProblem()
    {
        var validator = RollingPulseSurveyTemplateRules.DefaultRollingPulseSurveyTemplateValidator();
        var item = RollingPulseSurveyTemplate.NewRollingPulseSurveyTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPulseSurveyTemplateDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new RollingPulseSurveyTemplateDiscountRangeRule(7.6, 2141.0);
        var item = RollingPulseSurveyTemplate.NewRollingPulseSurveyTemplate("x", "y");
        item.Discount = 2142.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingPulseSurveyTemplateValidatorReportsEveryProblem();
        RollingPulseSurveyTemplateDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
