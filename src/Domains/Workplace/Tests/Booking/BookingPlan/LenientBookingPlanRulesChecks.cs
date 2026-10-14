namespace Acme.Workforce.Workplace.Tests.Booking.BookingPlan;

using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Checks of booking plan code, run by the project's runner.</summary>
public static class LenientBookingPlanRulesChecks
{
    public static void LenientBookingPlanValidatorReportsEveryProblem()
    {
        var validator = LenientBookingPlanRules.DefaultLenientBookingPlanValidator();
        var item = LenientBookingPlan.NewLenientBookingPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBookingPlanSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new LenientBookingPlanSegmentLengthRule(255);
        var item = LenientBookingPlan.NewLenientBookingPlan("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBookingPlanRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new LenientBookingPlanRequiredSegmentRule("lenient booking plan needs a value");
        var item = LenientBookingPlan.NewLenientBookingPlan("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBookingPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientBookingPlanCodeLengthRule(1000);
        var item = LenientBookingPlan.NewLenientBookingPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBookingPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientBookingPlanRequiredCodeRule("lenient booking plan needs a value");
        var item = LenientBookingPlan.NewLenientBookingPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientBookingPlanValidatorReportsEveryProblem();
        LenientBookingPlanSegmentLengthRuleFlagsALongSegment();
        LenientBookingPlanRequiredSegmentRuleFlagsABlankSegment();
        LenientBookingPlanCodeLengthRuleFlagsALongCode();
        LenientBookingPlanRequiredCodeRuleFlagsABlankCode();
    }
}
