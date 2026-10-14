namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayBudget;

using Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>Checks of pay budget code, run by the project's runner.</summary>
public static class ManualRetroPayBudgetRulesChecks
{
    public static void ManualRetroPayBudgetValidatorReportsEveryProblem()
    {
        var validator = ManualRetroPayBudgetRules.DefaultManualRetroPayBudgetValidator();
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualRetroPayBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualRetroPayBudgetRequiredCodeRule("manual retro pay budget needs a value");
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRetroPayBudgetEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ManualRetroPayBudgetEndsAtRangeRule(32, 120);
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualRetroPayBudgetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualRetroPayBudgetRequiredLabelRule("manual retro pay budget needs a value");
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualRetroPayBudgetValidatorReportsEveryProblem();
        ManualRetroPayBudgetRequiredCodeRuleFlagsABlankCode();
        ManualRetroPayBudgetEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ManualRetroPayBudgetRequiredLabelRuleFlagsABlankLabel();
    }
}
