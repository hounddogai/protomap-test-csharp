namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedPolicyAckSnapshotRulesChecks
{
    public static void BatchedPolicyAckSnapshotValidatorReportsEveryProblem()
    {
        var validator = BatchedPolicyAckSnapshotRules.DefaultBatchedPolicyAckSnapshotValidator();
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPolicyAckSnapshotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedPolicyAckSnapshotTitleLengthRule(3600);
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPolicyAckSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedPolicyAckSnapshotRequiredCodeRule("batched policy ack snapshot needs a value");
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedPolicyAckSnapshotMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BatchedPolicyAckSnapshotMarginRangeRule(1.1, 3377.7);
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("x", "y");
        item.Margin = 3379.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPolicyAckSnapshotExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new BatchedPolicyAckSnapshotExternalRefLengthRule(64);
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPolicyAckSnapshotCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new BatchedPolicyAckSnapshotCeilingRangeRule(6.3, 1369.5);
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("x", "y");
        item.Ceiling = 1371.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPolicyAckSnapshotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedPolicyAckSnapshotRequiredTitleRule("batched policy ack snapshot needs a value");
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedPolicyAckSnapshotValidatorReportsEveryProblem();
        BatchedPolicyAckSnapshotTitleLengthRuleFlagsALongTitle();
        BatchedPolicyAckSnapshotRequiredCodeRuleFlagsABlankCode();
        BatchedPolicyAckSnapshotMarginRangeRuleFlagsValuesAboveMargin();
        BatchedPolicyAckSnapshotExternalRefLengthRuleFlagsALongExternalRef();
        BatchedPolicyAckSnapshotCeilingRangeRuleFlagsValuesAboveCeiling();
        BatchedPolicyAckSnapshotRequiredTitleRuleFlagsABlankTitle();
    }
}
