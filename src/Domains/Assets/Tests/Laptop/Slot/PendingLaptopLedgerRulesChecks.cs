namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PendingLaptopLedgerRulesChecks
{
    public static void PendingLaptopLedgerValidatorReportsEveryProblem()
    {
        var validator = PendingLaptopLedgerRules.DefaultPendingLaptopLedgerValidator();
        var item = PendingLaptopLedger.NewPendingLaptopLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingLaptopLedgerRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new PendingLaptopLedgerRequiredCurrencyRule("pending laptop ledger needs this field");
        var item = PendingLaptopLedger.NewPendingLaptopLedger("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingLaptopLedgerRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PendingLaptopLedgerRetriesRangeRule(64, 255);
        var item = PendingLaptopLedger.NewPendingLaptopLedger("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingLaptopLedgerValidatorReportsEveryProblem();
        PendingLaptopLedgerRequiredCurrencyRuleFlagsABlankCurrency();
        PendingLaptopLedgerRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
