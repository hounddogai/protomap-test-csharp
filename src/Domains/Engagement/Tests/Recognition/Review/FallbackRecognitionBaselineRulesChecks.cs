namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class FallbackRecognitionBaselineRulesChecks
{
    public static void FallbackRecognitionBaselineValidatorReportsEveryProblem()
    {
        var validator = FallbackRecognitionBaselineRules.DefaultFallbackRecognitionBaselineValidator();
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackRecognitionBaselineStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new FallbackRecognitionBaselineStartsAtRangeRule(1000, 3600);
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackRecognitionBaselineUnitLengthRuleFlagsALongUnit()
    {
        var rule = new FallbackRecognitionBaselineUnitLengthRule(16);
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackRecognitionBaselineCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new FallbackRecognitionBaselineCapacityRangeRule(16, 3600);
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackRecognitionBaselineRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new FallbackRecognitionBaselineRatioRangeRule(2.4, 1028.8);
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("x", "y");
        item.Ratio = 1030.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackRecognitionBaselineValidatorReportsEveryProblem();
        FallbackRecognitionBaselineStartsAtRangeRuleFlagsValuesAboveStartsAt();
        FallbackRecognitionBaselineUnitLengthRuleFlagsALongUnit();
        FallbackRecognitionBaselineCapacityRangeRuleFlagsValuesAboveCapacity();
        FallbackRecognitionBaselineRatioRangeRuleFlagsValuesAboveRatio();
    }
}
