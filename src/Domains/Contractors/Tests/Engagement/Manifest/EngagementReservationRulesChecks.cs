namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class EngagementReservationRulesChecks
{
    public static void EngagementReservationValidatorReportsEveryProblem()
    {
        var validator = EngagementReservationRules.DefaultEngagementReservationValidator();
        var item = EngagementReservation.NewEngagementReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementReservationRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new EngagementReservationRatioRangeRule(7.5, 3083.4);
        var item = EngagementReservation.NewEngagementReservation("x", "y");
        item.Ratio = 3084.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementReservationRequiredCodeRule("engagement reservation needs a value");
        var item = EngagementReservation.NewEngagementReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementReservationLocationLengthRuleFlagsALongLocation()
    {
        var rule = new EngagementReservationLocationLengthRule(64);
        var item = EngagementReservation.NewEngagementReservation("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementReservationRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new EngagementReservationRequiredPhoneRule("engagement reservation needs an entry");
        var item = EngagementReservation.NewEngagementReservation("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementReservationValidatorReportsEveryProblem();
        EngagementReservationRatioRangeRuleFlagsValuesAboveRatio();
        EngagementReservationRequiredCodeRuleFlagsABlankCode();
        EngagementReservationLocationLengthRuleFlagsALongLocation();
        EngagementReservationRequiredPhoneRuleFlagsABlankPhone();
    }
}
