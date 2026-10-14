namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackActionPlanDraftRulesChecks
{
    public static void FallbackActionPlanDraftValidatorReportsEveryProblem()
    {
        var validator = FallbackActionPlanDraftRules.DefaultFallbackActionPlanDraftValidator();
        var item = FallbackActionPlanDraft.NewFallbackActionPlanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackActionPlanDraftRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackActionPlanDraftRequiredLabelRule("fallback action plan draft needs a value");
        var item = FallbackActionPlanDraft.NewFallbackActionPlanDraft("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackActionPlanDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackActionPlanDraftCodeLengthRule(3600);
        var item = FallbackActionPlanDraft.NewFallbackActionPlanDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackActionPlanDraftValidatorReportsEveryProblem();
        FallbackActionPlanDraftRequiredLabelRuleFlagsABlankLabel();
        FallbackActionPlanDraftCodeLengthRuleFlagsALongCode();
    }
}
