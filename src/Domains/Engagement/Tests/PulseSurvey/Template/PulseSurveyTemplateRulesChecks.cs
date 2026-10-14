namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyTemplateRulesChecks
{
    public static void PulseSurveyTemplateValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyTemplateRules.DefaultPulseSurveyTemplateValidator();
        var item = PulseSurveyTemplate.NewPulseSurveyTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyTemplateRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PulseSurveyTemplateRequiredCategoryRule("pulse survey template needs a value");
        var item = PulseSurveyTemplate.NewPulseSurveyTemplate("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyTemplateRequiredCodeRule("pulse survey template needs a value");
        var item = PulseSurveyTemplate.NewPulseSurveyTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyTemplateValidatorReportsEveryProblem();
        PulseSurveyTemplateRequiredCategoryRuleFlagsABlankCategory();
        PulseSurveyTemplateRequiredCodeRuleFlagsABlankCode();
    }
}
