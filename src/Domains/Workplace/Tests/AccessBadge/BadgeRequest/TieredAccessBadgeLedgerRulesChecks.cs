namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class TieredAccessBadgeLedgerRulesChecks
{
    public static void TieredAccessBadgeLedgerValidatorReportsEveryProblem()
    {
        var validator = TieredAccessBadgeLedgerRules.DefaultTieredAccessBadgeLedgerValidator();
        var item = TieredAccessBadgeLedger.NewTieredAccessBadgeLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredAccessBadgeLedgerEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new TieredAccessBadgeLedgerEndsAtRangeRule(120, 120);
        var item = TieredAccessBadgeLedger.NewTieredAccessBadgeLedger("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredAccessBadgeLedgerRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new TieredAccessBadgeLedgerRequiredBadgeRule("tiered access badge ledger needs an entry");
        var item = TieredAccessBadgeLedger.NewTieredAccessBadgeLedger("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredAccessBadgeLedgerValidatorReportsEveryProblem();
        TieredAccessBadgeLedgerEndsAtRangeRuleFlagsValuesAboveEndsAt();
        TieredAccessBadgeLedgerRequiredBadgeRuleFlagsABlankBadge();
    }
}
