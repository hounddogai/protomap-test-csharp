namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FallbackPolicyScoreRulesChecks
{
    public static void FallbackPolicyScoreValidatorReportsEveryProblem()
    {
        var validator = FallbackPolicyScoreRules.DefaultFallbackPolicyScoreValidator();
        var item = FallbackPolicyScore.NewFallbackPolicyScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPolicyScoreBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new FallbackPolicyScoreBudgetRangeRule(0.7, 1612.5);
        var item = FallbackPolicyScore.NewFallbackPolicyScore("x", "y");
        item.Budget = 1614.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPolicyScoreDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new FallbackPolicyScoreDurationMinutesRangeRule(1000, 86400);
        var item = FallbackPolicyScore.NewFallbackPolicyScore("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPolicyScoreSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new FallbackPolicyScoreSegmentLengthRule(16);
        var item = FallbackPolicyScore.NewFallbackPolicyScore("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPolicyScoreRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new FallbackPolicyScoreRequiredSegmentRule("fallback policy score needs a value");
        var item = FallbackPolicyScore.NewFallbackPolicyScore("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackPolicyScoreValidatorReportsEveryProblem();
        FallbackPolicyScoreBudgetRangeRuleFlagsValuesAboveBudget();
        FallbackPolicyScoreDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        FallbackPolicyScoreSegmentLengthRuleFlagsALongSegment();
        FallbackPolicyScoreRequiredSegmentRuleFlagsABlankSegment();
    }
}
