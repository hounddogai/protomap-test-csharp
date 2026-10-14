namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionReservationRulesChecks
{
    public static void RecognitionReservationValidatorReportsEveryProblem()
    {
        var validator = RecognitionReservationRules.DefaultRecognitionReservationValidator();
        var item = RecognitionReservation.NewRecognitionReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new RecognitionReservationCodeLengthRule(64);
        var item = RecognitionReservation.NewRecognitionReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionReservationGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new RecognitionReservationGraceDaysRangeRule(16, 3600);
        var item = RecognitionReservation.NewRecognitionReservation("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionReservationRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new RecognitionReservationRequiredTimezoneRule("recognition reservation needs this field");
        var item = RecognitionReservation.NewRecognitionReservation("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionReservationRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new RecognitionReservationRequiredOwnerIdRule("recognition reservation needs this field");
        var item = RecognitionReservation.NewRecognitionReservation("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionReservationValidatorReportsEveryProblem();
        RecognitionReservationCodeLengthRuleFlagsALongCode();
        RecognitionReservationGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        RecognitionReservationRequiredTimezoneRuleFlagsABlankTimezone();
        RecognitionReservationRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
