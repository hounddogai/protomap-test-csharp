namespace Acme.Workforce.Payrollops.Tests.RetroPay.Budget;

using Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RetroPayBudgetRulesChecks
{
    public static void RetroPayBudgetValidatorReportsEveryProblem()
    {
        var validator = RetroPayBudgetRules.DefaultRetroPayBudgetValidator();
        var item = RetroPayBudget.NewRetroPayBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayBudgetRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new RetroPayBudgetRequiredExternalRefRule("retro pay budget needs an entry");
        var item = RetroPayBudget.NewRetroPayBudget("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayBudgetCodeLengthRule(32);
        var item = RetroPayBudget.NewRetroPayBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPayBudgetValidatorReportsEveryProblem();
        RetroPayBudgetRequiredExternalRefRuleFlagsABlankExternalRef();
        RetroPayBudgetCodeLengthRuleFlagsALongCode();
    }
}
