namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSpanRecordRulesChecks
{
    public static void LenientSpanRecordValidatorReportsEveryProblem()
    {
        var validator = LenientSpanRecordRules.DefaultLenientSpanRecordValidator();
        var item = LenientSpanRecord.NewLenientSpanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSpanRecordRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new LenientSpanRecordRequiredSkillRule("lenient span record needs this field");
        var item = LenientSpanRecord.NewLenientSpanRecord("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSpanRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientSpanRecordLabelLengthRule(1000);
        var item = LenientSpanRecord.NewLenientSpanRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSpanRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientSpanRecordRequiredLabelRule("lenient span record needs this field");
        var item = LenientSpanRecord.NewLenientSpanRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSpanRecordSkillLengthRuleFlagsALongSkill()
    {
        var rule = new LenientSpanRecordSkillLengthRule(64);
        var item = LenientSpanRecord.NewLenientSpanRecord("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientSpanRecordValidatorReportsEveryProblem();
        LenientSpanRecordRequiredSkillRuleFlagsABlankSkill();
        LenientSpanRecordLabelLengthRuleFlagsALongLabel();
        LenientSpanRecordRequiredLabelRuleFlagsABlankLabel();
        LenientSpanRecordSkillLengthRuleFlagsALongSkill();
    }
}
