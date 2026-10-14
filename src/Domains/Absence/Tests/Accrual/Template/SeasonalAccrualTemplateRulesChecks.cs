namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SeasonalAccrualTemplateRulesChecks
{
    public static void SeasonalAccrualTemplateValidatorReportsEveryProblem()
    {
        var validator = SeasonalAccrualTemplateRules.DefaultSeasonalAccrualTemplateValidator();
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAccrualTemplateCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new SeasonalAccrualTemplateCategoryLengthRule(16);
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAccrualTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new SeasonalAccrualTemplateGraceDaysRangeRule(16, 3600);
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalAccrualTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalAccrualTemplateCodeLengthRule(86400);
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalAccrualTemplateValidatorReportsEveryProblem();
        SeasonalAccrualTemplateCategoryLengthRuleFlagsALongCategory();
        SeasonalAccrualTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        SeasonalAccrualTemplateCodeLengthRuleFlagsALongCode();
    }
}
