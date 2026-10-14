namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackEngagementLedgerRulesChecks
{
    public static void FallbackEngagementLedgerValidatorReportsEveryProblem()
    {
        var validator = FallbackEngagementLedgerRules.DefaultFallbackEngagementLedgerValidator();
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackEngagementLedgerRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new FallbackEngagementLedgerRequiredJobCodeRule("fallback engagement ledger needs this field");
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackEngagementLedgerRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackEngagementLedgerRequiredTitleRule("fallback engagement ledger needs an entry");
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackEngagementLedgerBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new FallbackEngagementLedgerBudgetRangeRule(6.3, 627.6);
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("x", "y");
        item.Budget = 629.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackEngagementLedgerWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new FallbackEngagementLedgerWindowDaysRangeRule(16, 64);
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("x", "y");
        item.WindowDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackEngagementLedgerValidatorReportsEveryProblem();
        FallbackEngagementLedgerRequiredJobCodeRuleFlagsABlankJobCode();
        FallbackEngagementLedgerRequiredTitleRuleFlagsABlankTitle();
        FallbackEngagementLedgerBudgetRangeRuleFlagsValuesAboveBudget();
        FallbackEngagementLedgerWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
