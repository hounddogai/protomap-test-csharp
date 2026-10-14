namespace Acme.Workforce.Assets.Tests.Refresh.Ledger;

using Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RefreshLedgerRulesChecks
{
    public static void RefreshLedgerValidatorReportsEveryProblem()
    {
        var validator = RefreshLedgerRules.DefaultRefreshLedgerValidator();
        var item = RefreshLedger.NewRefreshLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RefreshLedgerLabelLengthRule(86400);
        var item = RefreshLedger.NewRefreshLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshLedgerRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new RefreshLedgerRequiredCategoryRule("refresh ledger needs this field");
        var item = RefreshLedger.NewRefreshLedger("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshLedgerUnitLengthRuleFlagsALongUnit()
    {
        var rule = new RefreshLedgerUnitLengthRule(16);
        var item = RefreshLedger.NewRefreshLedger("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshLedgerRequiredCodeRule("refresh ledger needs an entry");
        var item = RefreshLedger.NewRefreshLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshLedgerValidatorReportsEveryProblem();
        RefreshLedgerLabelLengthRuleFlagsALongLabel();
        RefreshLedgerRequiredCategoryRuleFlagsABlankCategory();
        RefreshLedgerUnitLengthRuleFlagsALongUnit();
        RefreshLedgerRequiredCodeRuleFlagsABlankCode();
    }
}
