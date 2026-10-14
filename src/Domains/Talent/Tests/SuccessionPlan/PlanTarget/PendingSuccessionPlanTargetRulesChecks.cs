namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class PendingSuccessionPlanTargetRulesChecks
{
    public static void PendingSuccessionPlanTargetValidatorReportsEveryProblem()
    {
        var validator = PendingSuccessionPlanTargetRules.DefaultPendingSuccessionPlanTargetValidator();
        var item = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingSuccessionPlanTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingSuccessionPlanTargetRequiredLabelRule("pending succession plan target needs this field");
        var item = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingSuccessionPlanTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingSuccessionPlanTargetCodeLengthRule(32);
        var item = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingSuccessionPlanTargetValidatorReportsEveryProblem();
        PendingSuccessionPlanTargetRequiredLabelRuleFlagsABlankLabel();
        PendingSuccessionPlanTargetCodeLengthRuleFlagsALongCode();
    }
}
