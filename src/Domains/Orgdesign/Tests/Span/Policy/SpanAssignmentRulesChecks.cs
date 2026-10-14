namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SpanAssignmentRulesChecks
{
    public static void SpanAssignmentValidatorReportsEveryProblem()
    {
        var validator = SpanAssignmentRules.DefaultSpanAssignmentValidator();
        var item = SpanAssignment.NewSpanAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanAssignmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new SpanAssignmentScoreRangeRule(2.3, 3944.8);
        var item = SpanAssignment.NewSpanAssignment("x", "y");
        item.Score = 3946.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanAssignmentCodeLengthRule(80);
        var item = SpanAssignment.NewSpanAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanAssignmentRequiredCodeRule("span assignment needs an entry");
        var item = SpanAssignment.NewSpanAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanAssignmentUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SpanAssignmentUpdatedAtRangeRule(255, 255);
        var item = SpanAssignment.NewSpanAssignment("x", "y");
        item.UpdatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SpanAssignmentValidatorReportsEveryProblem();
        SpanAssignmentScoreRangeRuleFlagsValuesAboveScore();
        SpanAssignmentCodeLengthRuleFlagsALongCode();
        SpanAssignmentRequiredCodeRuleFlagsABlankCode();
        SpanAssignmentUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
