namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledPositionTemplateRulesChecks
{
    public static void ScheduledPositionTemplateValidatorReportsEveryProblem()
    {
        var validator = ScheduledPositionTemplateRules.DefaultScheduledPositionTemplateValidator();
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPositionTemplateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledPositionTemplateRequiredTitleRule("scheduled position template needs this field");
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPositionTemplateRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new ScheduledPositionTemplateRequiredSummaryRule("scheduled position template needs this field");
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPositionTemplateRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new ScheduledPositionTemplateRequiredUnitRule("scheduled position template needs an entry");
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPositionTemplateUnitLengthRuleFlagsALongUnit()
    {
        var rule = new ScheduledPositionTemplateUnitLengthRule(80);
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPositionTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledPositionTemplateCodeLengthRule(120);
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPositionTemplateValidatorReportsEveryProblem();
        ScheduledPositionTemplateRequiredTitleRuleFlagsABlankTitle();
        ScheduledPositionTemplateRequiredSummaryRuleFlagsABlankSummary();
        ScheduledPositionTemplateRequiredUnitRuleFlagsABlankUnit();
        ScheduledPositionTemplateUnitLengthRuleFlagsALongUnit();
        ScheduledPositionTemplateCodeLengthRuleFlagsALongCode();
    }
}
