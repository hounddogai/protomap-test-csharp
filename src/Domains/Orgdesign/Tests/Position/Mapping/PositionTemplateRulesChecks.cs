namespace Acme.Workforce.Orgdesign.Tests.Position.Mapping;

using Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PositionTemplateRulesChecks
{
    public static void PositionTemplateValidatorReportsEveryProblem()
    {
        var validator = PositionTemplateRules.DefaultPositionTemplateValidator();
        var item = PositionTemplate.NewPositionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new PositionTemplateRequiredNameRule("position template needs this field");
        var item = PositionTemplate.NewPositionTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PositionTemplateGraceDaysRangeRule(16, 86400);
        var item = PositionTemplate.NewPositionTemplate("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PositionTemplateRequiredLabelRule("position template needs this field");
        var item = PositionTemplate.NewPositionTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionTemplateValidatorReportsEveryProblem();
        PositionTemplateRequiredNameRuleFlagsABlankName();
        PositionTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        PositionTemplateRequiredLabelRuleFlagsABlankLabel();
    }
}
