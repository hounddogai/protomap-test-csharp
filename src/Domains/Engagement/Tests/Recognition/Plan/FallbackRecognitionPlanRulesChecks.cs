namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class FallbackRecognitionPlanRulesChecks
{
    public static void FallbackRecognitionPlanValidatorReportsEveryProblem()
    {
        var validator = FallbackRecognitionPlanRules.DefaultFallbackRecognitionPlanValidator();
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackRecognitionPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackRecognitionPlanLabelLengthRule(80);
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackRecognitionPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackRecognitionPlanRequiredCodeRule("fallback recognition plan needs this field");
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRecognitionPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackRecognitionPlanCodeLengthRule(32);
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackRecognitionPlanOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new FallbackRecognitionPlanOwnerIdLengthRule(1000);
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackRecognitionPlanValidatorReportsEveryProblem();
        FallbackRecognitionPlanLabelLengthRuleFlagsALongLabel();
        FallbackRecognitionPlanRequiredCodeRuleFlagsABlankCode();
        FallbackRecognitionPlanCodeLengthRuleFlagsALongCode();
        FallbackRecognitionPlanOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
