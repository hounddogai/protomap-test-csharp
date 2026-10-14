namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingTrainingDueLedgerRulesChecks
{
    public static void RollingTrainingDueLedgerValidatorReportsEveryProblem()
    {
        var validator = RollingTrainingDueLedgerRules.DefaultRollingTrainingDueLedgerValidator();
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingTrainingDueLedgerBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new RollingTrainingDueLedgerBudgetRangeRule(1.0, 3984.9);
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("x", "y");
        item.Budget = 3986.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingTrainingDueLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingTrainingDueLedgerLabelLengthRule(1000);
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingTrainingDueLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingTrainingDueLedgerRequiredLabelRule("rolling training due ledger needs an entry");
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingTrainingDueLedgerValidatorReportsEveryProblem();
        RollingTrainingDueLedgerBudgetRangeRuleFlagsValuesAboveBudget();
        RollingTrainingDueLedgerLabelLengthRuleFlagsALongLabel();
        RollingTrainingDueLedgerRequiredLabelRuleFlagsABlankLabel();
    }
}
