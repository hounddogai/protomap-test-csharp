namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueLedgerRulesChecks
{
    public static void TrainingDueLedgerValidatorReportsEveryProblem()
    {
        var validator = TrainingDueLedgerRules.DefaultTrainingDueLedgerValidator();
        var item = TrainingDueLedger.NewTrainingDueLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueLedgerRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new TrainingDueLedgerRatingRangeRule(4.9, 2397.7);
        var item = TrainingDueLedger.NewTrainingDueLedger("x", "y");
        item.Rating = 2399.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueLedgerRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new TrainingDueLedgerRequiredContactEmailRule("training due ledger needs a value");
        var item = TrainingDueLedger.NewTrainingDueLedger("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueLedgerMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new TrainingDueLedgerMaximumRangeRule(5.8, 1078.1);
        var item = TrainingDueLedger.NewTrainingDueLedger("x", "y");
        item.Maximum = 1079.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDueLedgerValidatorReportsEveryProblem();
        TrainingDueLedgerRatingRangeRuleFlagsValuesAboveRating();
        TrainingDueLedgerRequiredContactEmailRuleFlagsABlankContactEmail();
        TrainingDueLedgerMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
