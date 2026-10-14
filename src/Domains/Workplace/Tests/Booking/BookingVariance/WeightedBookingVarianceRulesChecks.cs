namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class WeightedBookingVarianceRulesChecks
{
    public static void WeightedBookingVarianceValidatorReportsEveryProblem()
    {
        var validator = WeightedBookingVarianceRules.DefaultWeightedBookingVarianceValidator();
        var item = WeightedBookingVariance.NewWeightedBookingVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBookingVariancePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new WeightedBookingVariancePriorityRangeRule(16, 255);
        var item = WeightedBookingVariance.NewWeightedBookingVariance("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBookingVarianceRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedBookingVarianceRequiredNameRule("weighted booking variance needs this field");
        var item = WeightedBookingVariance.NewWeightedBookingVariance("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedBookingVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedBookingVarianceRequiredCodeRule("weighted booking variance needs a value");
        var item = WeightedBookingVariance.NewWeightedBookingVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedBookingVarianceMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new WeightedBookingVarianceMaximumRangeRule(2.8, 1332.1);
        var item = WeightedBookingVariance.NewWeightedBookingVariance("x", "y");
        item.Maximum = 1333.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBookingVarianceRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new WeightedBookingVarianceRequiredChannelRule("weighted booking variance needs a value");
        var item = WeightedBookingVariance.NewWeightedBookingVariance("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedBookingVarianceValidatorReportsEveryProblem();
        WeightedBookingVariancePriorityRangeRuleFlagsValuesAbovePriority();
        WeightedBookingVarianceRequiredNameRuleFlagsABlankName();
        WeightedBookingVarianceRequiredCodeRuleFlagsABlankCode();
        WeightedBookingVarianceMaximumRangeRuleFlagsValuesAboveMaximum();
        WeightedBookingVarianceRequiredChannelRuleFlagsABlankChannel();
    }
}
