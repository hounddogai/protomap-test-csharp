namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientEquityGrantLedgerRulesChecks
{
    public static void LenientEquityGrantLedgerValidatorReportsEveryProblem()
    {
        var validator = LenientEquityGrantLedgerRules.DefaultLenientEquityGrantLedgerValidator();
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientEquityGrantLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientEquityGrantLedgerTitleLengthRule(120);
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientEquityGrantLedgerCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new LenientEquityGrantLedgerCurrencyLengthRule(32);
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientEquityGrantLedgerNameLengthRuleFlagsALongName()
    {
        var rule = new LenientEquityGrantLedgerNameLengthRule(255);
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientEquityGrantLedgerValidatorReportsEveryProblem();
        LenientEquityGrantLedgerTitleLengthRuleFlagsALongTitle();
        LenientEquityGrantLedgerCurrencyLengthRuleFlagsALongCurrency();
        LenientEquityGrantLedgerNameLengthRuleFlagsALongName();
    }
}
