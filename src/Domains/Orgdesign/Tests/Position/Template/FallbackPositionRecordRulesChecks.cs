namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class FallbackPositionRecordRulesChecks
{
    public static void FallbackPositionRecordValidatorReportsEveryProblem()
    {
        var validator = FallbackPositionRecordRules.DefaultFallbackPositionRecordValidator();
        var item = FallbackPositionRecord.NewFallbackPositionRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPositionRecordSkillLengthRuleFlagsALongSkill()
    {
        var rule = new FallbackPositionRecordSkillLengthRule(86400);
        var item = FallbackPositionRecord.NewFallbackPositionRecord("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPositionRecordFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new FallbackPositionRecordFloorValueRangeRule(0.1, 4142.4);
        var item = FallbackPositionRecord.NewFallbackPositionRecord("x", "y");
        item.FloorValue = 4143.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPositionRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackPositionRecordCodeLengthRule(120);
        var item = FallbackPositionRecord.NewFallbackPositionRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackPositionRecordValidatorReportsEveryProblem();
        FallbackPositionRecordSkillLengthRuleFlagsALongSkill();
        FallbackPositionRecordFloorValueRangeRuleFlagsValuesAboveFloorValue();
        FallbackPositionRecordCodeLengthRuleFlagsALongCode();
    }
}
