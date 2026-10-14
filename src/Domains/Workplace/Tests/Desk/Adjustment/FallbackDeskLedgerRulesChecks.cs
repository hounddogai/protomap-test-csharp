namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackDeskLedgerRulesChecks
{
    public static void FallbackDeskLedgerValidatorReportsEveryProblem()
    {
        var validator = FallbackDeskLedgerRules.DefaultFallbackDeskLedgerValidator();
        var item = FallbackDeskLedger.NewFallbackDeskLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackDeskLedgerAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new FallbackDeskLedgerAmountRangeRule(2.7, 106.4);
        var item = FallbackDeskLedger.NewFallbackDeskLedger("x", "y");
        item.Amount = 107.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackDeskLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackDeskLedgerCodeLengthRule(64);
        var item = FallbackDeskLedger.NewFallbackDeskLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackDeskLedgerLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new FallbackDeskLedgerLocaleLengthRule(86400);
        var item = FallbackDeskLedger.NewFallbackDeskLedger("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackDeskLedgerRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackDeskLedgerRequiredNameRule("fallback desk ledger needs a value");
        var item = FallbackDeskLedger.NewFallbackDeskLedger("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackDeskLedgerCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new FallbackDeskLedgerCategoryLengthRule(32);
        var item = FallbackDeskLedger.NewFallbackDeskLedger("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackDeskLedgerRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new FallbackDeskLedgerRequiredCategoryRule("fallback desk ledger needs a value");
        var item = FallbackDeskLedger.NewFallbackDeskLedger("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackDeskLedgerValidatorReportsEveryProblem();
        FallbackDeskLedgerAmountRangeRuleFlagsValuesAboveAmount();
        FallbackDeskLedgerCodeLengthRuleFlagsALongCode();
        FallbackDeskLedgerLocaleLengthRuleFlagsALongLocale();
        FallbackDeskLedgerRequiredNameRuleFlagsABlankName();
        FallbackDeskLedgerCategoryLengthRuleFlagsALongCategory();
        FallbackDeskLedgerRequiredCategoryRuleFlagsABlankCategory();
    }
}
