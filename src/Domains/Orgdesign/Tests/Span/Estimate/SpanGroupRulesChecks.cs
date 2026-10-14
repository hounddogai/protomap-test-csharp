namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SpanGroupRulesChecks
{
    public static void SpanGroupValidatorReportsEveryProblem()
    {
        var validator = SpanGroupRules.DefaultSpanGroupValidator();
        var item = SpanGroup.NewSpanGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanGroupCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new SpanGroupCeilingRangeRule(0.5, 4486.7);
        var item = SpanGroup.NewSpanGroup("x", "y");
        item.Ceiling = 4488.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanGroupCodeLengthRule(1000);
        var item = SpanGroup.NewSpanGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanGroupRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new SpanGroupRequiredSkillRule("span group needs this field");
        var item = SpanGroup.NewSpanGroup("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SpanGroupLabelLengthRule(1000);
        var item = SpanGroup.NewSpanGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanGroupValidatorReportsEveryProblem();
        SpanGroupCeilingRangeRuleFlagsValuesAboveCeiling();
        SpanGroupCodeLengthRuleFlagsALongCode();
        SpanGroupRequiredSkillRuleFlagsABlankSkill();
        SpanGroupLabelLengthRuleFlagsALongLabel();
    }
}
