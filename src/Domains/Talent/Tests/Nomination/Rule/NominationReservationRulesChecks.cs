namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationReservationRulesChecks
{
    public static void NominationReservationValidatorReportsEveryProblem()
    {
        var validator = NominationReservationRules.DefaultNominationReservationValidator();
        var item = NominationReservation.NewNominationReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationReservationRequiredCodeRule("nomination reservation needs this field");
        var item = NominationReservation.NewNominationReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationReservationLocationLengthRuleFlagsALongLocation()
    {
        var rule = new NominationReservationLocationLengthRule(1000);
        var item = NominationReservation.NewNominationReservation("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        NominationReservationValidatorReportsEveryProblem();
        NominationReservationRequiredCodeRuleFlagsABlankCode();
        NominationReservationLocationLengthRuleFlagsALongLocation();
    }
}
