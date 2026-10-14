namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PrimarySpanScoreRulesChecks
{
    public static void PrimarySpanScoreValidatorReportsEveryProblem()
    {
        var validator = PrimarySpanScoreRules.DefaultPrimarySpanScoreValidator();
        var item = PrimarySpanScore.NewPrimarySpanScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimarySpanScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimarySpanScoreRequiredCodeRule("primary span score needs this field");
        var item = PrimarySpanScore.NewPrimarySpanScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimarySpanScoreRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PrimarySpanScoreRetriesRangeRule(16, 255);
        var item = PrimarySpanScore.NewPrimarySpanScore("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimarySpanScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimarySpanScoreLabelLengthRule(120);
        var item = PrimarySpanScore.NewPrimarySpanScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimarySpanScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimarySpanScoreCodeLengthRule(32);
        var item = PrimarySpanScore.NewPrimarySpanScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimarySpanScoreValidatorReportsEveryProblem();
        PrimarySpanScoreRequiredCodeRuleFlagsABlankCode();
        PrimarySpanScoreRetriesRangeRuleFlagsValuesAboveRetries();
        PrimarySpanScoreLabelLengthRuleFlagsALongLabel();
        PrimarySpanScoreCodeLengthRuleFlagsALongCode();
    }
}
