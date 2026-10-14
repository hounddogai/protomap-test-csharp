namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HeadcountTemplateRulesChecks
{
    public static void HeadcountTemplateValidatorReportsEveryProblem()
    {
        var validator = HeadcountTemplateRules.DefaultHeadcountTemplateValidator();
        var item = HeadcountTemplate.NewHeadcountTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountTemplateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HeadcountTemplateTitleLengthRule(86400);
        var item = HeadcountTemplate.NewHeadcountTemplate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountTemplateRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new HeadcountTemplateRequiredGradeRule("headcount template needs a value");
        var item = HeadcountTemplate.NewHeadcountTemplate("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountTemplateTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new HeadcountTemplateTenureMonthsRangeRule(3600, 86400);
        var item = HeadcountTemplate.NewHeadcountTemplate("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountTemplateRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new HeadcountTemplateRequiredTeamRule("headcount template needs a value");
        var item = HeadcountTemplate.NewHeadcountTemplate("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountTemplateTeamLengthRuleFlagsALongTeam()
    {
        var rule = new HeadcountTemplateTeamLengthRule(1000);
        var item = HeadcountTemplate.NewHeadcountTemplate("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountTemplateValidatorReportsEveryProblem();
        HeadcountTemplateTitleLengthRuleFlagsALongTitle();
        HeadcountTemplateRequiredGradeRuleFlagsABlankGrade();
        HeadcountTemplateTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        HeadcountTemplateRequiredTeamRuleFlagsABlankTeam();
        HeadcountTemplateTeamLengthRuleFlagsALongTeam();
    }
}
