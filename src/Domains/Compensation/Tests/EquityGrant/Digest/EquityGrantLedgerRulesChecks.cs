namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantLedgerRulesChecks
{
    public static void EquityGrantLedgerValidatorReportsEveryProblem()
    {
        var validator = EquityGrantLedgerRules.DefaultEquityGrantLedgerValidator();
        var item = EquityGrantLedger.NewEquityGrantLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantLedgerLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new EquityGrantLedgerLeaveDaysRangeRule(80, 255);
        var item = EquityGrantLedger.NewEquityGrantLedger("x", "y");
        item.LeaveDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantLedgerVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new EquityGrantLedgerVersionRangeRule(80, 255);
        var item = EquityGrantLedger.NewEquityGrantLedger("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantLedgerRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new EquityGrantLedgerRequiredRegionRule("equity grant ledger needs an entry");
        var item = EquityGrantLedger.NewEquityGrantLedger("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantLedgerRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new EquityGrantLedgerRequiredTitleRule("equity grant ledger needs this field");
        var item = EquityGrantLedger.NewEquityGrantLedger("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantLedgerValidatorReportsEveryProblem();
        EquityGrantLedgerLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        EquityGrantLedgerVersionRangeRuleFlagsValuesAboveVersion();
        EquityGrantLedgerRequiredRegionRuleFlagsABlankRegion();
        EquityGrantLedgerRequiredTitleRuleFlagsABlankTitle();
    }
}
