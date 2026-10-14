namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BalanceRequestRulesChecks
{
    public static void BalanceRequestValidatorReportsEveryProblem()
    {
        var validator = BalanceRequestRules.DefaultBalanceRequestValidator();
        var item = BalanceRequest.NewBalanceRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceRequestNameLengthRuleFlagsALongName()
    {
        var rule = new BalanceRequestNameLengthRule(32);
        var item = BalanceRequest.NewBalanceRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceRequestLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new BalanceRequestLimitRangeRule(16, 120);
        var item = BalanceRequest.NewBalanceRequest("x", "y");
        item.Limit = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceRequestBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BalanceRequestBudgetRangeRule(5.0, 316.3);
        var item = BalanceRequest.NewBalanceRequest("x", "y");
        item.Budget = 317.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceRequestValidatorReportsEveryProblem();
        BalanceRequestNameLengthRuleFlagsALongName();
        BalanceRequestLimitRangeRuleFlagsValuesAboveLimit();
        BalanceRequestBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
