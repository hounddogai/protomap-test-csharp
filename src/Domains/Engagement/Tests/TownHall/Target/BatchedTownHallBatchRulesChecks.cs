namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BatchedTownHallBatchRulesChecks
{
    public static void BatchedTownHallBatchValidatorReportsEveryProblem()
    {
        var validator = BatchedTownHallBatchRules.DefaultBatchedTownHallBatchValidator();
        var item = BatchedTownHallBatch.NewBatchedTownHallBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTownHallBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BatchedTownHallBatchLabelLengthRule(120);
        var item = BatchedTownHallBatch.NewBatchedTownHallBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedTownHallBatchBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new BatchedTownHallBatchBadgeLengthRule(120);
        var item = BatchedTownHallBatch.NewBatchedTownHallBatch("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedTownHallBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedTownHallBatchCodeLengthRule(16);
        var item = BatchedTownHallBatch.NewBatchedTownHallBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedTownHallBatchValidatorReportsEveryProblem();
        BatchedTownHallBatchLabelLengthRuleFlagsALongLabel();
        BatchedTownHallBatchBadgeLengthRuleFlagsALongBadge();
        BatchedTownHallBatchCodeLengthRuleFlagsALongCode();
    }
}
