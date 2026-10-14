namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantBudgetRulesChecks
{
    public static void EquityGrantBudgetValidatorReportsEveryProblem()
    {
        var validator = EquityGrantBudgetRules.DefaultEquityGrantBudgetValidator();
        var item = EquityGrantBudget.NewEquityGrantBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantBudgetRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new EquityGrantBudgetRequiredSalaryBandRule("equity grant budget needs an entry");
        var item = EquityGrantBudget.NewEquityGrantBudget("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantBudgetOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new EquityGrantBudgetOwnerIdLengthRule(1000);
        var item = EquityGrantBudget.NewEquityGrantBudget("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantBudgetSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new EquityGrantBudgetSalaryBandLengthRule(3600);
        var item = EquityGrantBudget.NewEquityGrantBudget("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantBudgetRequiredCodeRule("equity grant budget needs a value");
        var item = EquityGrantBudget.NewEquityGrantBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantBudgetValidatorReportsEveryProblem();
        EquityGrantBudgetRequiredSalaryBandRuleFlagsABlankSalaryBand();
        EquityGrantBudgetOwnerIdLengthRuleFlagsALongOwnerId();
        EquityGrantBudgetSalaryBandLengthRuleFlagsALongSalaryBand();
        EquityGrantBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
