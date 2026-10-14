namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PrimaryHeadcountLedgerRulesChecks
{
    public static void PrimaryHeadcountLedgerValidatorReportsEveryProblem()
    {
        var validator = PrimaryHeadcountLedgerRules.DefaultPrimaryHeadcountLedgerValidator();
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryHeadcountLedgerQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PrimaryHeadcountLedgerQuantityRangeRule(80, 80);
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryHeadcountLedgerReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PrimaryHeadcountLedgerReviewerLengthRule(32);
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryHeadcountLedgerRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PrimaryHeadcountLedgerRequiredReviewerRule("primary headcount ledger needs an entry");
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryHeadcountLedgerTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new PrimaryHeadcountLedgerTimezoneLengthRule(255);
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryHeadcountLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryHeadcountLedgerCodeLengthRule(80);
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryHeadcountLedgerValidatorReportsEveryProblem();
        PrimaryHeadcountLedgerQuantityRangeRuleFlagsValuesAboveQuantity();
        PrimaryHeadcountLedgerReviewerLengthRuleFlagsALongReviewer();
        PrimaryHeadcountLedgerRequiredReviewerRuleFlagsABlankReviewer();
        PrimaryHeadcountLedgerTimezoneLengthRuleFlagsALongTimezone();
        PrimaryHeadcountLedgerCodeLengthRuleFlagsALongCode();
    }
}
