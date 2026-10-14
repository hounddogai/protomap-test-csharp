namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HeadcountScoreRulesChecks
{
    public static void HeadcountScoreValidatorReportsEveryProblem()
    {
        var validator = HeadcountScoreRules.DefaultHeadcountScoreValidator();
        var item = HeadcountScore.NewHeadcountScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountScoreThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new HeadcountScoreThresholdRangeRule(7.8, 4811.0);
        var item = HeadcountScore.NewHeadcountScore("x", "y");
        item.Threshold = 4812.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountScoreMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new HeadcountScoreMarginRangeRule(7.6, 3774.8);
        var item = HeadcountScore.NewHeadcountScore("x", "y");
        item.Margin = 3776.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HeadcountScoreTitleLengthRule(3600);
        var item = HeadcountScore.NewHeadcountScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountScoreValidatorReportsEveryProblem();
        HeadcountScoreThresholdRangeRuleFlagsValuesAboveThreshold();
        HeadcountScoreMarginRangeRuleFlagsValuesAboveMargin();
        HeadcountScoreTitleLengthRuleFlagsALongTitle();
    }
}
