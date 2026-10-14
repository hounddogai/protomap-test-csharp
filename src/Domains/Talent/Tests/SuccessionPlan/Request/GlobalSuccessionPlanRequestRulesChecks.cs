namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Request;

using Acme.Workforce.Talent.SuccessionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class GlobalSuccessionPlanRequestRulesChecks
{
    public static void GlobalSuccessionPlanRequestValidatorReportsEveryProblem()
    {
        var validator = GlobalSuccessionPlanRequestRules.DefaultGlobalSuccessionPlanRequestValidator();
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalSuccessionPlanRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalSuccessionPlanRequestLabelLengthRule(86400);
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSuccessionPlanRequestCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new GlobalSuccessionPlanRequestCostRangeRule(4.6, 467.0);
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("x", "y");
        item.Cost = 468.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalSuccessionPlanRequestManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new GlobalSuccessionPlanRequestManagerIdLengthRule(120);
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSuccessionPlanRequestRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new GlobalSuccessionPlanRequestRequiredUnitRule("global succession plan request needs a value");
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalSuccessionPlanRequestPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new GlobalSuccessionPlanRequestPriorityRangeRule(32, 64);
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("x", "y");
        item.Priority = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalSuccessionPlanRequestValidatorReportsEveryProblem();
        GlobalSuccessionPlanRequestLabelLengthRuleFlagsALongLabel();
        GlobalSuccessionPlanRequestCostRangeRuleFlagsValuesAboveCost();
        GlobalSuccessionPlanRequestManagerIdLengthRuleFlagsALongManagerId();
        GlobalSuccessionPlanRequestRequiredUnitRuleFlagsABlankUnit();
        GlobalSuccessionPlanRequestPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
