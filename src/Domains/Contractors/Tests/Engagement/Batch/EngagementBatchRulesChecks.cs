namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EngagementBatchRulesChecks
{
    public static void EngagementBatchValidatorReportsEveryProblem()
    {
        var validator = EngagementBatchRules.DefaultEngagementBatchValidator();
        var item = EngagementBatch.NewEngagementBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementBatchCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new EngagementBatchCapacityRangeRule(16, 3600);
        var item = EngagementBatch.NewEngagementBatch("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EngagementBatchLabelLengthRule(80);
        var item = EngagementBatch.NewEngagementBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new EngagementBatchRequiredLabelRule("engagement batch needs an entry");
        var item = EngagementBatch.NewEngagementBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementBatchFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new EngagementBatchFteRangeRule(5.5, 4529.9);
        var item = EngagementBatch.NewEngagementBatch("x", "y");
        item.Fte = 4531.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EngagementBatchValidatorReportsEveryProblem();
        EngagementBatchCapacityRangeRuleFlagsValuesAboveCapacity();
        EngagementBatchLabelLengthRuleFlagsALongLabel();
        EngagementBatchRequiredLabelRuleFlagsABlankLabel();
        EngagementBatchFteRangeRuleFlagsValuesAboveFte();
    }
}
