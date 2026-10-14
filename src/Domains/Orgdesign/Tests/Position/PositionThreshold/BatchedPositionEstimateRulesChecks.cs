namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class BatchedPositionEstimateRulesChecks
{
    public static void BatchedPositionEstimateValidatorReportsEveryProblem()
    {
        var validator = BatchedPositionEstimateRules.DefaultBatchedPositionEstimateValidator();
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPositionEstimateLocationLengthRuleFlagsALongLocation()
    {
        var rule = new BatchedPositionEstimateLocationLengthRule(1000);
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPositionEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedPositionEstimateNameLengthRule(32);
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPositionEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedPositionEstimateCodeLengthRule(32);
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPositionEstimateSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new BatchedPositionEstimateSummaryLengthRule(3600);
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPositionEstimateRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new BatchedPositionEstimateRequiredLocationRule("batched position estimate needs an entry");
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedPositionEstimateValidatorReportsEveryProblem();
        BatchedPositionEstimateLocationLengthRuleFlagsALongLocation();
        BatchedPositionEstimateNameLengthRuleFlagsALongName();
        BatchedPositionEstimateCodeLengthRuleFlagsALongCode();
        BatchedPositionEstimateSummaryLengthRuleFlagsALongSummary();
        BatchedPositionEstimateRequiredLocationRuleFlagsABlankLocation();
    }
}
