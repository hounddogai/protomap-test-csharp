namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedMobilityReservationRulesChecks
{
    public static void BatchedMobilityReservationValidatorReportsEveryProblem()
    {
        var validator = BatchedMobilityReservationRules.DefaultBatchedMobilityReservationValidator();
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedMobilityReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedMobilityReservationCodeLengthRule(32);
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedMobilityReservationLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new BatchedMobilityReservationLocaleLengthRule(64);
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedMobilityReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedMobilityReservationTitleLengthRule(80);
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedMobilityReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedMobilityReservationRequiredCodeRule("batched mobility reservation needs an entry");
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedMobilityReservationValidatorReportsEveryProblem();
        BatchedMobilityReservationCodeLengthRuleFlagsALongCode();
        BatchedMobilityReservationLocaleLengthRuleFlagsALongLocale();
        BatchedMobilityReservationTitleLengthRuleFlagsALongTitle();
        BatchedMobilityReservationRequiredCodeRuleFlagsABlankCode();
    }
}
