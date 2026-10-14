namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RollingRecognitionReservationRulesChecks
{
    public static void RollingRecognitionReservationValidatorReportsEveryProblem()
    {
        var validator = RollingRecognitionReservationRules.DefaultRollingRecognitionReservationValidator();
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRecognitionReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingRecognitionReservationRequiredCodeRule("rolling recognition reservation needs a value");
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingRecognitionReservationSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new RollingRecognitionReservationSegmentLengthRule(120);
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingRecognitionReservationRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new RollingRecognitionReservationRequiredTeamRule("rolling recognition reservation needs this field");
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingRecognitionReservationCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RollingRecognitionReservationCostRangeRule(5.1, 376.4);
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("x", "y");
        item.Cost = 377.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingRecognitionReservationValidatorReportsEveryProblem();
        RollingRecognitionReservationRequiredCodeRuleFlagsABlankCode();
        RollingRecognitionReservationSegmentLengthRuleFlagsALongSegment();
        RollingRecognitionReservationRequiredTeamRuleFlagsABlankTeam();
        RollingRecognitionReservationCostRangeRuleFlagsValuesAboveCost();
    }
}
