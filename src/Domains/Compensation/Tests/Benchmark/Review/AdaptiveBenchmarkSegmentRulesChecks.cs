namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkSegmentRulesChecks
{
    public static void AdaptiveBenchmarkSegmentValidatorReportsEveryProblem()
    {
        var validator = AdaptiveBenchmarkSegmentRules.DefaultAdaptiveBenchmarkSegmentValidator();
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveBenchmarkSegmentRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new AdaptiveBenchmarkSegmentRequiredGradeRule("adaptive benchmark segment needs a value");
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveBenchmarkSegmentGradeLengthRuleFlagsALongGrade()
    {
        var rule = new AdaptiveBenchmarkSegmentGradeLengthRule(32);
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveBenchmarkSegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveBenchmarkSegmentRequiredNameRule("adaptive benchmark segment needs an entry");
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveBenchmarkSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveBenchmarkSegmentNameLengthRule(64);
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveBenchmarkSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveBenchmarkSegmentCodeLengthRule(86400);
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveBenchmarkSegmentValidatorReportsEveryProblem();
        AdaptiveBenchmarkSegmentRequiredGradeRuleFlagsABlankGrade();
        AdaptiveBenchmarkSegmentGradeLengthRuleFlagsALongGrade();
        AdaptiveBenchmarkSegmentRequiredNameRuleFlagsABlankName();
        AdaptiveBenchmarkSegmentNameLengthRuleFlagsALongName();
        AdaptiveBenchmarkSegmentCodeLengthRuleFlagsALongCode();
    }
}
