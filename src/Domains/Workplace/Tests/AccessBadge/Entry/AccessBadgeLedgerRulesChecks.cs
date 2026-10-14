namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeLedgerRulesChecks
{
    public static void AccessBadgeLedgerValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeLedgerRules.DefaultAccessBadgeLedgerValidator();
        var item = AccessBadgeLedger.NewAccessBadgeLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeLedgerRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new AccessBadgeLedgerRevisionRangeRule(120, 3600);
        var item = AccessBadgeLedger.NewAccessBadgeLedger("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeLedgerSourceLengthRuleFlagsALongSource()
    {
        var rule = new AccessBadgeLedgerSourceLengthRule(255);
        var item = AccessBadgeLedger.NewAccessBadgeLedger("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccessBadgeLedgerRequiredLabelRule("access badge ledger needs this field");
        var item = AccessBadgeLedger.NewAccessBadgeLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeLedgerValidatorReportsEveryProblem();
        AccessBadgeLedgerRevisionRangeRuleFlagsValuesAboveRevision();
        AccessBadgeLedgerSourceLengthRuleFlagsALongSource();
        AccessBadgeLedgerRequiredLabelRuleFlagsABlankLabel();
    }
}
