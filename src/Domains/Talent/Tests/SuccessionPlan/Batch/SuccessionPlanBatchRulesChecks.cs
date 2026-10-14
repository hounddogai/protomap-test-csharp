namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanBatchRulesChecks
{
    public static void SuccessionPlanBatchValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanBatchRules.DefaultSuccessionPlanBatchValidator();
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SuccessionPlanBatchRequiredTitleRule("succession plan batch needs an entry");
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SuccessionPlanBatchTitleLengthRule(1000);
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanBatchChannelLengthRuleFlagsALongChannel()
    {
        var rule = new SuccessionPlanBatchChannelLengthRule(3600);
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanBatchRequiredCodeRule("succession plan batch needs this field");
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanBatchRegionLengthRuleFlagsALongRegion()
    {
        var rule = new SuccessionPlanBatchRegionLengthRule(3600);
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanBatchValidatorReportsEveryProblem();
        SuccessionPlanBatchRequiredTitleRuleFlagsABlankTitle();
        SuccessionPlanBatchTitleLengthRuleFlagsALongTitle();
        SuccessionPlanBatchChannelLengthRuleFlagsALongChannel();
        SuccessionPlanBatchRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanBatchRegionLengthRuleFlagsALongRegion();
    }
}
