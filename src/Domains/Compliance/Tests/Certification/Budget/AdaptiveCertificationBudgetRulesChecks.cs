namespace Acme.Workforce.Compliance.Tests.Certification.Budget;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveCertificationBudgetRulesChecks
{
    public static void AdaptiveCertificationBudgetValidatorReportsEveryProblem()
    {
        var validator = AdaptiveCertificationBudgetRules.DefaultAdaptiveCertificationBudgetValidator();
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveCertificationBudgetMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AdaptiveCertificationBudgetMarginRangeRule(6.5, 2058.8);
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("x", "y");
        item.Margin = 2060.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveCertificationBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveCertificationBudgetRequiredCodeRule("adaptive certification budget needs a value");
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveCertificationBudgetWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AdaptiveCertificationBudgetWindowDaysRangeRule(80, 86400);
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveCertificationBudgetFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AdaptiveCertificationBudgetFteRangeRule(6.8, 3751.6);
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("x", "y");
        item.Fte = 3753.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveCertificationBudgetValidatorReportsEveryProblem();
        AdaptiveCertificationBudgetMarginRangeRuleFlagsValuesAboveMargin();
        AdaptiveCertificationBudgetRequiredCodeRuleFlagsABlankCode();
        AdaptiveCertificationBudgetWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AdaptiveCertificationBudgetFteRangeRuleFlagsValuesAboveFte();
    }
}
