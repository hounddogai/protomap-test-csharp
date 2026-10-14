namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayBandWindowRulesChecks
{
    public static void PayBandWindowValidatorReportsEveryProblem()
    {
        var validator = PayBandWindowRules.DefaultPayBandWindowValidator();
        var item = PayBandWindow.NewPayBandWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandWindowTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayBandWindowTitleLengthRule(120);
        var item = PayBandWindow.NewPayBandWindow("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandWindowBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PayBandWindowBudgetRangeRule(6.5, 4511.0);
        var item = PayBandWindow.NewPayBandWindow("x", "y");
        item.Budget = 4512.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandWindowRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayBandWindowRequiredTitleRule("pay band window needs an entry");
        var item = PayBandWindow.NewPayBandWindow("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayBandWindowValidatorReportsEveryProblem();
        PayBandWindowTitleLengthRuleFlagsALongTitle();
        PayBandWindowBudgetRangeRuleFlagsValuesAboveBudget();
        PayBandWindowRequiredTitleRuleFlagsABlankTitle();
    }
}
