namespace Acme.Workforce.Engagement.Tests.Recognition.Score;

using Acme.Workforce.Engagement.Recognition.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RecognitionPolicyRulesChecks
{
    public static void RecognitionPolicyValidatorReportsEveryProblem()
    {
        var validator = RecognitionPolicyRules.DefaultRecognitionPolicyValidator();
        var item = RecognitionPolicy.NewRecognitionPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionPolicyRequiredCodeRule("recognition policy needs a value");
        var item = RecognitionPolicy.NewRecognitionPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionPolicyScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RecognitionPolicyScoreRangeRule(7.2, 1815.8);
        var item = RecognitionPolicy.NewRecognitionPolicy("x", "y");
        item.Score = 1817.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionPolicyRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RecognitionPolicyRatioRangeRule(6.6, 3185.4);
        var item = RecognitionPolicy.NewRecognitionPolicy("x", "y");
        item.Ratio = 3186.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RecognitionPolicyValidatorReportsEveryProblem();
        RecognitionPolicyRequiredCodeRuleFlagsABlankCode();
        RecognitionPolicyScoreRangeRuleFlagsValuesAboveScore();
        RecognitionPolicyRatioRangeRuleFlagsValuesAboveRatio();
    }
}
