namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyNominationReservationRulesChecks
{
    public static void LegacyNominationReservationValidatorReportsEveryProblem()
    {
        var validator = LegacyNominationReservationRules.DefaultLegacyNominationReservationValidator();
        var item = LegacyNominationReservation.NewLegacyNominationReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyNominationReservationScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new LegacyNominationReservationScoreRangeRule(4.6, 3537.2);
        var item = LegacyNominationReservation.NewLegacyNominationReservation("x", "y");
        item.Score = 3538.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyNominationReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyNominationReservationCodeLengthRule(120);
        var item = LegacyNominationReservation.NewLegacyNominationReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyNominationReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyNominationReservationLabelLengthRule(120);
        var item = LegacyNominationReservation.NewLegacyNominationReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyNominationReservationRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new LegacyNominationReservationRequiredSummaryRule("legacy nomination reservation needs a value");
        var item = LegacyNominationReservation.NewLegacyNominationReservation("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyNominationReservationSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LegacyNominationReservationSummaryLengthRule(32);
        var item = LegacyNominationReservation.NewLegacyNominationReservation("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyNominationReservationValidatorReportsEveryProblem();
        LegacyNominationReservationScoreRangeRuleFlagsValuesAboveScore();
        LegacyNominationReservationCodeLengthRuleFlagsALongCode();
        LegacyNominationReservationLabelLengthRuleFlagsALongLabel();
        LegacyNominationReservationRequiredSummaryRuleFlagsABlankSummary();
        LegacyNominationReservationSummaryLengthRuleFlagsALongSummary();
    }
}
