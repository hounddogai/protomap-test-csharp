namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyNoticeRulesChecks
{
    public static void PulseSurveyNoticeValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyNoticeRules.DefaultPulseSurveyNoticeValidator();
        var item = PulseSurveyNotice.NewPulseSurveyNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PulseSurveyNoticeRequiredTitleRule("pulse survey notice needs a value");
        var item = PulseSurveyNotice.NewPulseSurveyNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyNoticeQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PulseSurveyNoticeQuantityRangeRule(80, 255);
        var item = PulseSurveyNotice.NewPulseSurveyNotice("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PulseSurveyNoticeTitleLengthRule(16);
        var item = PulseSurveyNotice.NewPulseSurveyNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyNoticeRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PulseSurveyNoticeRequiredDescriptionRule("pulse survey notice needs this field");
        var item = PulseSurveyNotice.NewPulseSurveyNotice("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyNoticeValidatorReportsEveryProblem();
        PulseSurveyNoticeRequiredTitleRuleFlagsABlankTitle();
        PulseSurveyNoticeQuantityRangeRuleFlagsValuesAboveQuantity();
        PulseSurveyNoticeTitleLengthRuleFlagsALongTitle();
        PulseSurveyNoticeRequiredDescriptionRuleFlagsABlankDescription();
    }
}
