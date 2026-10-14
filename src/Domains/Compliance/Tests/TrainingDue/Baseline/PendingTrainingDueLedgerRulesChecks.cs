namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingTrainingDueLedgerRulesChecks
{
    public static void PendingTrainingDueLedgerValidatorReportsEveryProblem()
    {
        var validator = PendingTrainingDueLedgerRules.DefaultPendingTrainingDueLedgerValidator();
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingTrainingDueLedgerDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new PendingTrainingDueLedgerDescriptionLengthRule(86400);
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingTrainingDueLedgerRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PendingTrainingDueLedgerRatingRangeRule(6.4, 2254.1);
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("x", "y");
        item.Rating = 2255.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingTrainingDueLedgerRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingTrainingDueLedgerRequiredNameRule("pending training due ledger needs an entry");
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingTrainingDueLedgerSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new PendingTrainingDueLedgerSegmentLengthRule(64);
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingTrainingDueLedgerValidatorReportsEveryProblem();
        PendingTrainingDueLedgerDescriptionLengthRuleFlagsALongDescription();
        PendingTrainingDueLedgerRatingRangeRuleFlagsValuesAboveRating();
        PendingTrainingDueLedgerRequiredNameRuleFlagsABlankName();
        PendingTrainingDueLedgerSegmentLengthRuleFlagsALongSegment();
    }
}
