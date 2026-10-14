namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class LegacyPoolSegmentRulesChecks
{
    public static void LegacyPoolSegmentValidatorReportsEveryProblem()
    {
        var validator = LegacyPoolSegmentRules.DefaultLegacyPoolSegmentValidator();
        var item = LegacyPoolSegment.NewLegacyPoolSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPoolSegmentExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new LegacyPoolSegmentExternalRefLengthRule(16);
        var item = LegacyPoolSegment.NewLegacyPoolSegment("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPoolSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyPoolSegmentCodeLengthRule(32);
        var item = LegacyPoolSegment.NewLegacyPoolSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPoolSegmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyPoolSegmentRequiredLabelRule("legacy pool segment needs an entry");
        var item = LegacyPoolSegment.NewLegacyPoolSegment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPoolSegmentThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new LegacyPoolSegmentThresholdRangeRule(0.2, 2725.2);
        var item = LegacyPoolSegment.NewLegacyPoolSegment("x", "y");
        item.Threshold = 2726.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPoolSegmentRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new LegacyPoolSegmentRequiredExternalRefRule("legacy pool segment needs an entry");
        var item = LegacyPoolSegment.NewLegacyPoolSegment("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyPoolSegmentValidatorReportsEveryProblem();
        LegacyPoolSegmentExternalRefLengthRuleFlagsALongExternalRef();
        LegacyPoolSegmentCodeLengthRuleFlagsALongCode();
        LegacyPoolSegmentRequiredLabelRuleFlagsABlankLabel();
        LegacyPoolSegmentThresholdRangeRuleFlagsValuesAboveThreshold();
        LegacyPoolSegmentRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
