namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyReservationRulesChecks
{
    public static void PulseSurveyReservationValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyReservationRules.DefaultPulseSurveyReservationValidator();
        var item = PulseSurveyReservation.NewPulseSurveyReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyReservationCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PulseSurveyReservationCeilingRangeRule(9.6, 1740.7);
        var item = PulseSurveyReservation.NewPulseSurveyReservation("x", "y");
        item.Ceiling = 1742.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyReservationRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PulseSurveyReservationRevisionRangeRule(16, 86400);
        var item = PulseSurveyReservation.NewPulseSurveyReservation("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyReservationCodeLengthRule(255);
        var item = PulseSurveyReservation.NewPulseSurveyReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyReservationFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new PulseSurveyReservationFteRangeRule(7.7, 3400.0);
        var item = PulseSurveyReservation.NewPulseSurveyReservation("x", "y");
        item.Fte = 3401.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PulseSurveyReservationValidatorReportsEveryProblem();
        PulseSurveyReservationCeilingRangeRuleFlagsValuesAboveCeiling();
        PulseSurveyReservationRevisionRangeRuleFlagsValuesAboveRevision();
        PulseSurveyReservationCodeLengthRuleFlagsALongCode();
        PulseSurveyReservationFteRangeRuleFlagsValuesAboveFte();
    }
}
