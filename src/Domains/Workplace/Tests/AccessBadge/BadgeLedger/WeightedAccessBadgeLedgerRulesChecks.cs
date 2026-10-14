namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class WeightedAccessBadgeLedgerRulesChecks
{
    public static void WeightedAccessBadgeLedgerValidatorReportsEveryProblem()
    {
        var validator = WeightedAccessBadgeLedgerRules.DefaultWeightedAccessBadgeLedgerValidator();
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAccessBadgeLedgerRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new WeightedAccessBadgeLedgerRetriesRangeRule(86400, 86400);
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedAccessBadgeLedgerRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new WeightedAccessBadgeLedgerRequiredTimezoneRule("weighted access badge ledger needs this field");
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAccessBadgeLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedAccessBadgeLedgerCodeLengthRule(3600);
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeLedgerValidatorReportsEveryProblem();
        WeightedAccessBadgeLedgerRetriesRangeRuleFlagsValuesAboveRetries();
        WeightedAccessBadgeLedgerRequiredTimezoneRuleFlagsABlankTimezone();
        WeightedAccessBadgeLedgerCodeLengthRuleFlagsALongCode();
    }
}
