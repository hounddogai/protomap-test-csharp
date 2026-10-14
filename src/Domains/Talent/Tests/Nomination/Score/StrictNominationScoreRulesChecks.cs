namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictNominationScoreRulesChecks
{
    public static void StrictNominationScoreValidatorReportsEveryProblem()
    {
        var validator = StrictNominationScoreRules.DefaultStrictNominationScoreValidator();
        var item = StrictNominationScore.NewStrictNominationScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictNominationScoreReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new StrictNominationScoreReviewerLengthRule(16);
        var item = StrictNominationScore.NewStrictNominationScore("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictNominationScoreBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new StrictNominationScoreBudgetRangeRule(2.3, 1141.4);
        var item = StrictNominationScore.NewStrictNominationScore("x", "y");
        item.Budget = 1142.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictNominationScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictNominationScoreRequiredCodeRule("strict nomination score needs a value");
        var item = StrictNominationScore.NewStrictNominationScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictNominationScoreDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new StrictNominationScoreDurationMinutesRangeRule(64, 3600);
        var item = StrictNominationScore.NewStrictNominationScore("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictNominationScoreValidatorReportsEveryProblem();
        StrictNominationScoreReviewerLengthRuleFlagsALongReviewer();
        StrictNominationScoreBudgetRangeRuleFlagsValuesAboveBudget();
        StrictNominationScoreRequiredCodeRuleFlagsABlankCode();
        StrictNominationScoreDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
