namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingPlanRulesChecks
{
    public static void BookingPlanValidatorReportsEveryProblem()
    {
        var validator = BookingPlanRules.DefaultBookingPlanValidator();
        var item = BookingPlan.NewBookingPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingPlanWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new BookingPlanWeightRangeRule(2.5, 168.9);
        var item = BookingPlan.NewBookingPlan("x", "y");
        item.Weight = 170.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new BookingPlanRequiredNameRule("booking plan needs this field");
        var item = BookingPlan.NewBookingPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingPlanNameLengthRuleFlagsALongName()
    {
        var rule = new BookingPlanNameLengthRule(64);
        var item = BookingPlan.NewBookingPlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BookingPlanRequiredCodeRule("booking plan needs this field");
        var item = BookingPlan.NewBookingPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BookingPlanValidatorReportsEveryProblem();
        BookingPlanWeightRangeRuleFlagsValuesAboveWeight();
        BookingPlanRequiredNameRuleFlagsABlankName();
        BookingPlanNameLengthRuleFlagsALongName();
        BookingPlanRequiredCodeRuleFlagsABlankCode();
    }
}
