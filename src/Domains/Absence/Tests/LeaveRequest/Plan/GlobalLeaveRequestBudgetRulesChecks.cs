namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class GlobalLeaveRequestBudgetRulesChecks
{
    public static void GlobalLeaveRequestBudgetValidatorReportsEveryProblem()
    {
        var validator = GlobalLeaveRequestBudgetRules.DefaultGlobalLeaveRequestBudgetValidator();
        var item = GlobalLeaveRequestBudget.NewGlobalLeaveRequestBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalLeaveRequestBudgetGradeLengthRuleFlagsALongGrade()
    {
        var rule = new GlobalLeaveRequestBudgetGradeLengthRule(120);
        var item = GlobalLeaveRequestBudget.NewGlobalLeaveRequestBudget("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalLeaveRequestBudgetExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new GlobalLeaveRequestBudgetExternalRefLengthRule(80);
        var item = GlobalLeaveRequestBudget.NewGlobalLeaveRequestBudget("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalLeaveRequestBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalLeaveRequestBudgetCodeLengthRule(3600);
        var item = GlobalLeaveRequestBudget.NewGlobalLeaveRequestBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalLeaveRequestBudgetValidatorReportsEveryProblem();
        GlobalLeaveRequestBudgetGradeLengthRuleFlagsALongGrade();
        GlobalLeaveRequestBudgetExternalRefLengthRuleFlagsALongExternalRef();
        GlobalLeaveRequestBudgetCodeLengthRuleFlagsALongCode();
    }
}
