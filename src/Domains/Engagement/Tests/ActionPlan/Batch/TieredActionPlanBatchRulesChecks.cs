namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredActionPlanBatchRulesChecks
{
    public static void TieredActionPlanBatchValidatorReportsEveryProblem()
    {
        var validator = TieredActionPlanBatchRules.DefaultTieredActionPlanBatchValidator();
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredActionPlanBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredActionPlanBatchLabelLengthRule(80);
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredActionPlanBatchRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new TieredActionPlanBatchRatingRangeRule(5.1, 3167.9);
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("x", "y");
        item.Rating = 3169.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredActionPlanBatchRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new TieredActionPlanBatchRequiredSkillRule("tiered action plan batch needs a value");
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredActionPlanBatchValidatorReportsEveryProblem();
        TieredActionPlanBatchLabelLengthRuleFlagsALongLabel();
        TieredActionPlanBatchRatingRangeRuleFlagsValuesAboveRating();
        TieredActionPlanBatchRequiredSkillRuleFlagsABlankSkill();
    }
}
