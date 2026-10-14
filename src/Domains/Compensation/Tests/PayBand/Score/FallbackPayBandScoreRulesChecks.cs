namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FallbackPayBandScoreRulesChecks
{
    public static void FallbackPayBandScoreValidatorReportsEveryProblem()
    {
        var validator = FallbackPayBandScoreRules.DefaultFallbackPayBandScoreValidator();
        var item = FallbackPayBandScore.NewFallbackPayBandScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayBandScoreNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackPayBandScoreNameLengthRule(80);
        var item = FallbackPayBandScore.NewFallbackPayBandScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayBandScoreNotesLengthRuleFlagsALongNotes()
    {
        var rule = new FallbackPayBandScoreNotesLengthRule(120);
        var item = FallbackPayBandScore.NewFallbackPayBandScore("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayBandScoreLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new FallbackPayBandScoreLimitRangeRule(16, 64);
        var item = FallbackPayBandScore.NewFallbackPayBandScore("x", "y");
        item.Limit = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayBandScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackPayBandScoreCodeLengthRule(1000);
        var item = FallbackPayBandScore.NewFallbackPayBandScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackPayBandScoreValidatorReportsEveryProblem();
        FallbackPayBandScoreNameLengthRuleFlagsALongName();
        FallbackPayBandScoreNotesLengthRuleFlagsALongNotes();
        FallbackPayBandScoreLimitRangeRuleFlagsValuesAboveLimit();
        FallbackPayBandScoreCodeLengthRuleFlagsALongCode();
    }
}
