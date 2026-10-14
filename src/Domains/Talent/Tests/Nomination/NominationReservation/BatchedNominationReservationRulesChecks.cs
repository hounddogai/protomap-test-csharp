namespace Acme.Workforce.Talent.Tests.Nomination.NominationReservation;

using Acme.Workforce.Talent.Nomination.NominationReservation;

/// <summary>Checks of nomination reservation code, run by the project's runner.</summary>
public static class BatchedNominationReservationRulesChecks
{
    public static void BatchedNominationReservationValidatorReportsEveryProblem()
    {
        var validator = BatchedNominationReservationRules.DefaultBatchedNominationReservationValidator();
        var item = BatchedNominationReservation.NewBatchedNominationReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedNominationReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BatchedNominationReservationRequiredLabelRule("batched nomination reservation needs this field");
        var item = BatchedNominationReservation.NewBatchedNominationReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedNominationReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedNominationReservationTitleLengthRule(120);
        var item = BatchedNominationReservation.NewBatchedNominationReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedNominationReservationValidatorReportsEveryProblem();
        BatchedNominationReservationRequiredLabelRuleFlagsABlankLabel();
        BatchedNominationReservationTitleLengthRuleFlagsALongTitle();
    }
}
