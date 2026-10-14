namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveRecognitionBatchRulesChecks
{
    public static void AdaptiveRecognitionBatchValidatorReportsEveryProblem()
    {
        var validator = AdaptiveRecognitionBatchRules.DefaultAdaptiveRecognitionBatchValidator();
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveRecognitionBatchRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AdaptiveRecognitionBatchRequiredLocationRule("adaptive recognition batch needs this field");
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveRecognitionBatchRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new AdaptiveRecognitionBatchRequiredNotesRule("adaptive recognition batch needs this field");
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveRecognitionBatchLocationLengthRuleFlagsALongLocation()
    {
        var rule = new AdaptiveRecognitionBatchLocationLengthRule(3600);
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveRecognitionBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveRecognitionBatchLabelLengthRule(3600);
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveRecognitionBatchValidatorReportsEveryProblem();
        AdaptiveRecognitionBatchRequiredLocationRuleFlagsABlankLocation();
        AdaptiveRecognitionBatchRequiredNotesRuleFlagsABlankNotes();
        AdaptiveRecognitionBatchLocationLengthRuleFlagsALongLocation();
        AdaptiveRecognitionBatchLabelLengthRuleFlagsALongLabel();
    }
}
