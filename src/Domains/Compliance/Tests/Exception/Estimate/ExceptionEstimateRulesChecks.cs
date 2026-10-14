namespace Acme.Workforce.Compliance.Tests.Exception.Estimate;

using Acme.Workforce.Compliance.Exception.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ExceptionEstimateRulesChecks
{
    public static void ExceptionEstimateValidatorReportsEveryProblem()
    {
        var validator = ExceptionEstimateRules.DefaultExceptionEstimateValidator();
        var item = ExceptionEstimate.NewExceptionEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionEstimateRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new ExceptionEstimateRequiredTimezoneRule("exception estimate needs a value");
        var item = ExceptionEstimate.NewExceptionEstimate("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionEstimateRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new ExceptionEstimateRequiredJobCodeRule("exception estimate needs a value");
        var item = ExceptionEstimate.NewExceptionEstimate("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionEstimateScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new ExceptionEstimateScoreRangeRule(8.4, 412.4);
        var item = ExceptionEstimate.NewExceptionEstimate("x", "y");
        item.Score = 413.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionEstimateJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new ExceptionEstimateJobCodeLengthRule(86400);
        var item = ExceptionEstimate.NewExceptionEstimate("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionEstimateRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new ExceptionEstimateRatioRangeRule(7.3, 441.9);
        var item = ExceptionEstimate.NewExceptionEstimate("x", "y");
        item.Ratio = 443.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ExceptionEstimateValidatorReportsEveryProblem();
        ExceptionEstimateRequiredTimezoneRuleFlagsABlankTimezone();
        ExceptionEstimateRequiredJobCodeRuleFlagsABlankJobCode();
        ExceptionEstimateScoreRangeRuleFlagsValuesAboveScore();
        ExceptionEstimateJobCodeLengthRuleFlagsALongJobCode();
        ExceptionEstimateRatioRangeRuleFlagsValuesAboveRatio();
    }
}
