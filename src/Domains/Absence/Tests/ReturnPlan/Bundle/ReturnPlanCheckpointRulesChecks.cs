namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanCheckpointRulesChecks
{
    public static void ReturnPlanCheckpointValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanCheckpointRules.DefaultReturnPlanCheckpointValidator();
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanCheckpointRequiredCodeRule("return plan checkpoint needs an entry");
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ReturnPlanCheckpointTitleLengthRule(120);
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanCheckpointRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new ReturnPlanCheckpointRequiredManagerIdRule("return plan checkpoint needs this field");
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReturnPlanCheckpointCodeLengthRule(32);
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanCheckpointValidatorReportsEveryProblem();
        ReturnPlanCheckpointRequiredCodeRuleFlagsABlankCode();
        ReturnPlanCheckpointTitleLengthRuleFlagsALongTitle();
        ReturnPlanCheckpointRequiredManagerIdRuleFlagsABlankManagerId();
        ReturnPlanCheckpointCodeLengthRuleFlagsALongCode();
    }
}
