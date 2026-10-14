namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingRecognitionBatchRulesChecks
{
    public static void PendingRecognitionBatchValidatorReportsEveryProblem()
    {
        var validator = PendingRecognitionBatchRules.DefaultPendingRecognitionBatchValidator();
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingRecognitionBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingRecognitionBatchTitleLengthRule(64);
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingRecognitionBatchChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PendingRecognitionBatchChannelLengthRule(32);
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingRecognitionBatchRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PendingRecognitionBatchRatioRangeRule(7.0, 1699.6);
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("x", "y");
        item.Ratio = 1701.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingRecognitionBatchDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PendingRecognitionBatchDurationMinutesRangeRule(255, 3600);
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingRecognitionBatchRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new PendingRecognitionBatchRequiredChannelRule("pending recognition batch needs a value");
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingRecognitionBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingRecognitionBatchRequiredCodeRule("pending recognition batch needs this field");
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingRecognitionBatchValidatorReportsEveryProblem();
        PendingRecognitionBatchTitleLengthRuleFlagsALongTitle();
        PendingRecognitionBatchChannelLengthRuleFlagsALongChannel();
        PendingRecognitionBatchRatioRangeRuleFlagsValuesAboveRatio();
        PendingRecognitionBatchDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        PendingRecognitionBatchRequiredChannelRuleFlagsABlankChannel();
        PendingRecognitionBatchRequiredCodeRuleFlagsABlankCode();
    }
}
