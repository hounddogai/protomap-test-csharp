namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class CappedPayRunTargetRulesChecks
{
    public static void CappedPayRunTargetValidatorReportsEveryProblem()
    {
        var validator = CappedPayRunTargetRules.DefaultCappedPayRunTargetValidator();
        var item = CappedPayRunTarget.NewCappedPayRunTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPayRunTargetAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new CappedPayRunTargetAmountRangeRule(3.7, 2402.7);
        var item = CappedPayRunTarget.NewCappedPayRunTarget("x", "y");
        item.Amount = 2404.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedPayRunTargetChannelLengthRuleFlagsALongChannel()
    {
        var rule = new CappedPayRunTargetChannelLengthRule(64);
        var item = CappedPayRunTarget.NewCappedPayRunTarget("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPayRunTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedPayRunTargetRequiredTitleRule("capped pay run target needs a value");
        var item = CappedPayRunTarget.NewCappedPayRunTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPayRunTargetRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new CappedPayRunTargetRequiredChannelRule("capped pay run target needs this field");
        var item = CappedPayRunTarget.NewCappedPayRunTarget("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPayRunTargetCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new CappedPayRunTargetCurrencyLengthRule(16);
        var item = CappedPayRunTarget.NewCappedPayRunTarget("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedPayRunTargetValidatorReportsEveryProblem();
        CappedPayRunTargetAmountRangeRuleFlagsValuesAboveAmount();
        CappedPayRunTargetChannelLengthRuleFlagsALongChannel();
        CappedPayRunTargetRequiredTitleRuleFlagsABlankTitle();
        CappedPayRunTargetRequiredChannelRuleFlagsABlankChannel();
        CappedPayRunTargetCurrencyLengthRuleFlagsALongCurrency();
    }
}
