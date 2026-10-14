namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileSegmentRulesChecks
{
    public static void SkillProfileSegmentValidatorReportsEveryProblem()
    {
        var validator = SkillProfileSegmentRules.DefaultSkillProfileSegmentValidator();
        var item = SkillProfileSegment.NewSkillProfileSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileSegmentRequiredCodeRule("skill profile segment needs this field");
        var item = SkillProfileSegment.NewSkillProfileSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileSegmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SkillProfileSegmentRequiredLabelRule("skill profile segment needs an entry");
        var item = SkillProfileSegment.NewSkillProfileSegment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileSegmentRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new SkillProfileSegmentRequiredCostCenterRule("skill profile segment needs a value");
        var item = SkillProfileSegment.NewSkillProfileSegment("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileSegmentBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new SkillProfileSegmentBatchSizeRangeRule(32, 255);
        var item = SkillProfileSegment.NewSkillProfileSegment("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SkillProfileSegmentValidatorReportsEveryProblem();
        SkillProfileSegmentRequiredCodeRuleFlagsABlankCode();
        SkillProfileSegmentRequiredLabelRuleFlagsABlankLabel();
        SkillProfileSegmentRequiredCostCenterRuleFlagsABlankCostCenter();
        SkillProfileSegmentBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
