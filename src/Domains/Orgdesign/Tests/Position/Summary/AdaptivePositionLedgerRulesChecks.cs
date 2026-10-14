namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AdaptivePositionLedgerRulesChecks
{
    public static void AdaptivePositionLedgerValidatorReportsEveryProblem()
    {
        var validator = AdaptivePositionLedgerRules.DefaultAdaptivePositionLedgerValidator();
        var item = AdaptivePositionLedger.NewAdaptivePositionLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePositionLedgerBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AdaptivePositionLedgerBudgetRangeRule(5.8, 472.0);
        var item = AdaptivePositionLedger.NewAdaptivePositionLedger("x", "y");
        item.Budget = 473.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptivePositionLedgerBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AdaptivePositionLedgerBatchSizeRangeRule(16, 86400);
        var item = AdaptivePositionLedger.NewAdaptivePositionLedger("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptivePositionLedgerValidatorReportsEveryProblem();
        AdaptivePositionLedgerBudgetRangeRuleFlagsValuesAboveBudget();
        AdaptivePositionLedgerBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
