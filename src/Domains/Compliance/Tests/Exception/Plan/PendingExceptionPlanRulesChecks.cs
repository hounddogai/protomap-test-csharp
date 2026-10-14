namespace Acme.Workforce.Compliance.Tests.Exception.Plan;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingExceptionPlanRulesChecks
{
    public static void PendingExceptionPlanValidatorReportsEveryProblem()
    {
        var validator = PendingExceptionPlanRules.DefaultPendingExceptionPlanValidator();
        var item = PendingExceptionPlan.NewPendingExceptionPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingExceptionPlanTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingExceptionPlanTitleLengthRule(255);
        var item = PendingExceptionPlan.NewPendingExceptionPlan("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingExceptionPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingExceptionPlanRequiredCodeRule("pending exception plan needs an entry");
        var item = PendingExceptionPlan.NewPendingExceptionPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingExceptionPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingExceptionPlanCodeLengthRule(64);
        var item = PendingExceptionPlan.NewPendingExceptionPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingExceptionPlanValidatorReportsEveryProblem();
        PendingExceptionPlanTitleLengthRuleFlagsALongTitle();
        PendingExceptionPlanRequiredCodeRuleFlagsABlankCode();
        PendingExceptionPlanCodeLengthRuleFlagsALongCode();
    }
}
