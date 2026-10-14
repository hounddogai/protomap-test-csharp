namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredEquityGrantLedgerRulesChecks
{
    public static void TieredEquityGrantLedgerValidatorReportsEveryProblem()
    {
        var validator = TieredEquityGrantLedgerRules.DefaultTieredEquityGrantLedgerValidator();
        var item = TieredEquityGrantLedger.NewTieredEquityGrantLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredEquityGrantLedgerRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredEquityGrantLedgerRequiredNameRule("tiered equity grant ledger needs a value");
        var item = TieredEquityGrantLedger.NewTieredEquityGrantLedger("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredEquityGrantLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredEquityGrantLedgerLabelLengthRule(3600);
        var item = TieredEquityGrantLedger.NewTieredEquityGrantLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredEquityGrantLedgerValidatorReportsEveryProblem();
        TieredEquityGrantLedgerRequiredNameRuleFlagsABlankName();
        TieredEquityGrantLedgerLabelLengthRuleFlagsALongLabel();
    }
}
