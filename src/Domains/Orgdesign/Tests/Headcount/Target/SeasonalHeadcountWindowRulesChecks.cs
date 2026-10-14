namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalHeadcountWindowRulesChecks
{
    public static void SeasonalHeadcountWindowValidatorReportsEveryProblem()
    {
        var validator = SeasonalHeadcountWindowRules.DefaultSeasonalHeadcountWindowValidator();
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalHeadcountWindowRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalHeadcountWindowRequiredLabelRule("seasonal headcount window needs an entry");
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalHeadcountWindowRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new SeasonalHeadcountWindowRequiredSkillRule("seasonal headcount window needs this field");
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalHeadcountWindowJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new SeasonalHeadcountWindowJobCodeLengthRule(120);
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalHeadcountWindowMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SeasonalHeadcountWindowMarginRangeRule(6.5, 2442.0);
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("x", "y");
        item.Margin = 2443.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalHeadcountWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new SeasonalHeadcountWindowWindowDaysRangeRule(3600, 3600);
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalHeadcountWindowValidatorReportsEveryProblem();
        SeasonalHeadcountWindowRequiredLabelRuleFlagsABlankLabel();
        SeasonalHeadcountWindowRequiredSkillRuleFlagsABlankSkill();
        SeasonalHeadcountWindowJobCodeLengthRuleFlagsALongJobCode();
        SeasonalHeadcountWindowMarginRangeRuleFlagsValuesAboveMargin();
        SeasonalHeadcountWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
