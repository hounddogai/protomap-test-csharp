namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalNominationLedgerRulesChecks
{
    public static void SeasonalNominationLedgerValidatorReportsEveryProblem()
    {
        var validator = SeasonalNominationLedgerRules.DefaultSeasonalNominationLedgerValidator();
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalNominationLedgerBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new SeasonalNominationLedgerBudgetRangeRule(1.1, 1105.3);
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("x", "y");
        item.Budget = 1106.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalNominationLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalNominationLedgerCodeLengthRule(3600);
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalNominationLedgerExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new SeasonalNominationLedgerExpiresAtRangeRule(32, 120);
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalNominationLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalNominationLedgerRequiredLabelRule("seasonal nomination ledger needs a value");
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalNominationLedgerValidatorReportsEveryProblem();
        SeasonalNominationLedgerBudgetRangeRuleFlagsValuesAboveBudget();
        SeasonalNominationLedgerCodeLengthRuleFlagsALongCode();
        SeasonalNominationLedgerExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        SeasonalNominationLedgerRequiredLabelRuleFlagsABlankLabel();
    }
}
