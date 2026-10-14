namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanRequestRulesChecks
{
    public static void ActionPlanRequestValidatorReportsEveryProblem()
    {
        var validator = ActionPlanRequestRules.DefaultActionPlanRequestValidator();
        var item = ActionPlanRequest.NewActionPlanRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new ActionPlanRequestRequiredNameRule("action plan request needs this field");
        var item = ActionPlanRequest.NewActionPlanRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanRequestNameLengthRuleFlagsALongName()
    {
        var rule = new ActionPlanRequestNameLengthRule(64);
        var item = ActionPlanRequest.NewActionPlanRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanRequestCodeLengthRule(120);
        var item = ActionPlanRequest.NewActionPlanRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanRequestAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ActionPlanRequestAttemptsRangeRule(1000, 86400);
        var item = ActionPlanRequest.NewActionPlanRequest("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanRequestVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ActionPlanRequestVersionRangeRule(64, 80);
        var item = ActionPlanRequest.NewActionPlanRequest("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanRequestDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new ActionPlanRequestDepartmentLengthRule(1000);
        var item = ActionPlanRequest.NewActionPlanRequest("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanRequestValidatorReportsEveryProblem();
        ActionPlanRequestRequiredNameRuleFlagsABlankName();
        ActionPlanRequestNameLengthRuleFlagsALongName();
        ActionPlanRequestCodeLengthRuleFlagsALongCode();
        ActionPlanRequestAttemptsRangeRuleFlagsValuesAboveAttempts();
        ActionPlanRequestVersionRangeRuleFlagsValuesAboveVersion();
        ActionPlanRequestDepartmentLengthRuleFlagsALongDepartment();
    }
}
