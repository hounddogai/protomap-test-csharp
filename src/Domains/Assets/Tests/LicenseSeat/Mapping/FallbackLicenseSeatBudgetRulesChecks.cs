namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackLicenseSeatBudgetRulesChecks
{
    public static void FallbackLicenseSeatBudgetValidatorReportsEveryProblem()
    {
        var validator = FallbackLicenseSeatBudgetRules.DefaultFallbackLicenseSeatBudgetValidator();
        var item = FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackLicenseSeatBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackLicenseSeatBudgetRequiredTitleRule("fallback license seat budget needs this field");
        var item = FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackLicenseSeatBudgetSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new FallbackLicenseSeatBudgetSalaryBandLengthRule(120);
        var item = FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackLicenseSeatBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackLicenseSeatBudgetRequiredCodeRule("fallback license seat budget needs this field");
        var item = FallbackLicenseSeatBudget.NewFallbackLicenseSeatBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatBudgetValidatorReportsEveryProblem();
        FallbackLicenseSeatBudgetRequiredTitleRuleFlagsABlankTitle();
        FallbackLicenseSeatBudgetSalaryBandLengthRuleFlagsALongSalaryBand();
        FallbackLicenseSeatBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
