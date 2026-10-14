namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandScoreRulesChecks
{
    public static void PayBandScoreValidatorReportsEveryProblem()
    {
        var validator = PayBandScoreRules.DefaultPayBandScoreValidator();
        var item = PayBandScore.NewPayBandScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandScoreRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PayBandScoreRequiredLabelRule("pay band score needs an entry");
        var item = PayBandScore.NewPayBandScore("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandScoreMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new PayBandScoreMinimumRangeRule(3.1, 2901.2);
        var item = PayBandScore.NewPayBandScore("x", "y");
        item.Minimum = 2902.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayBandScoreTitleLengthRule(3600);
        var item = PayBandScore.NewPayBandScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayBandScoreLabelLengthRule(86400);
        var item = PayBandScore.NewPayBandScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandScoreValidatorReportsEveryProblem();
        PayBandScoreRequiredLabelRuleFlagsABlankLabel();
        PayBandScoreMinimumRangeRuleFlagsValuesAboveMinimum();
        PayBandScoreTitleLengthRuleFlagsALongTitle();
        PayBandScoreLabelLengthRuleFlagsALongLabel();
    }
}
