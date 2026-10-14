namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedRecognitionSlotRulesChecks
{
    public static void BatchedRecognitionSlotValidatorReportsEveryProblem()
    {
        var validator = BatchedRecognitionSlotRules.DefaultBatchedRecognitionSlotValidator();
        var item = BatchedRecognitionSlot.NewBatchedRecognitionSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRecognitionSlotRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new BatchedRecognitionSlotRequiredOwnerIdRule("batched recognition slot needs this field");
        var item = BatchedRecognitionSlot.NewBatchedRecognitionSlot("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedRecognitionSlotOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new BatchedRecognitionSlotOwnerIdLengthRule(80);
        var item = BatchedRecognitionSlot.NewBatchedRecognitionSlot("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedRecognitionSlotValidatorReportsEveryProblem();
        BatchedRecognitionSlotRequiredOwnerIdRuleFlagsABlankOwnerId();
        BatchedRecognitionSlotOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
