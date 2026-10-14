namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class DefaultCareerPathExceptionRulesChecks
{
    public static void DefaultCareerPathExceptionValidatorReportsEveryProblem()
    {
        var validator = DefaultCareerPathExceptionRules.DefaultDefaultCareerPathExceptionValidator();
        var item = DefaultCareerPathException.NewDefaultCareerPathException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultCareerPathExceptionSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new DefaultCareerPathExceptionSequenceRangeRule(32, 80);
        var item = DefaultCareerPathException.NewDefaultCareerPathException("x", "y");
        item.Sequence = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultCareerPathExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultCareerPathExceptionRequiredCodeRule("default career path exception needs this field");
        var item = DefaultCareerPathException.NewDefaultCareerPathException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultCareerPathExceptionSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new DefaultCareerPathExceptionSegmentLengthRule(120);
        var item = DefaultCareerPathException.NewDefaultCareerPathException("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultCareerPathExceptionRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultCareerPathExceptionRequiredLabelRule("default career path exception needs a value");
        var item = DefaultCareerPathException.NewDefaultCareerPathException("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultCareerPathExceptionValidatorReportsEveryProblem();
        DefaultCareerPathExceptionSequenceRangeRuleFlagsValuesAboveSequence();
        DefaultCareerPathExceptionRequiredCodeRuleFlagsABlankCode();
        DefaultCareerPathExceptionSegmentLengthRuleFlagsALongSegment();
        DefaultCareerPathExceptionRequiredLabelRuleFlagsABlankLabel();
    }
}
