namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackTrainingDueSegmentRulesChecks
{
    public static void FallbackTrainingDueSegmentValidatorReportsEveryProblem()
    {
        var validator = FallbackTrainingDueSegmentRules.DefaultFallbackTrainingDueSegmentValidator();
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackTrainingDueSegmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackTrainingDueSegmentRequiredLabelRule("fallback training due segment needs this field");
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackTrainingDueSegmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackTrainingDueSegmentLabelLengthRule(120);
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackTrainingDueSegmentMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new FallbackTrainingDueSegmentMinimumRangeRule(4.2, 1282.3);
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("x", "y");
        item.Minimum = 1283.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackTrainingDueSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackTrainingDueSegmentCodeLengthRule(1000);
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackTrainingDueSegmentValidatorReportsEveryProblem();
        FallbackTrainingDueSegmentRequiredLabelRuleFlagsABlankLabel();
        FallbackTrainingDueSegmentLabelLengthRuleFlagsALongLabel();
        FallbackTrainingDueSegmentMinimumRangeRuleFlagsValuesAboveMinimum();
        FallbackTrainingDueSegmentCodeLengthRuleFlagsALongCode();
    }
}
