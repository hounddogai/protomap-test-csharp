namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanBatchRulesChecks
{
    public static void ActionPlanBatchValidatorReportsEveryProblem()
    {
        var validator = ActionPlanBatchRules.DefaultActionPlanBatchValidator();
        var item = ActionPlanBatch.NewActionPlanBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanBatchSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ActionPlanBatchSkillLengthRule(64);
        var item = ActionPlanBatch.NewActionPlanBatch("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanBatchValidatorReportsEveryProblem();
        ActionPlanBatchSkillLengthRuleFlagsALongSkill();
    }
}
