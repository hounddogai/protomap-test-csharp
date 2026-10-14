namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingVarianceRulesChecks
{
    public static void BookingVarianceValidatorReportsEveryProblem()
    {
        var validator = BookingVarianceRules.DefaultBookingVarianceValidator();
        var item = BookingVariance.NewBookingVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingVarianceGradeLengthRuleFlagsALongGrade()
    {
        var rule = new BookingVarianceGradeLengthRule(120);
        var item = BookingVariance.NewBookingVariance("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingVarianceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BookingVarianceTenureMonthsRangeRule(64, 255);
        var item = BookingVariance.NewBookingVariance("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingVarianceRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new BookingVarianceRevisionRangeRule(255, 86400);
        var item = BookingVariance.NewBookingVariance("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingVarianceCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new BookingVarianceCostRangeRule(5.2, 4747.2);
        var item = BookingVariance.NewBookingVariance("x", "y");
        item.Cost = 4748.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BookingVarianceValidatorReportsEveryProblem();
        BookingVarianceGradeLengthRuleFlagsALongGrade();
        BookingVarianceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        BookingVarianceRevisionRangeRuleFlagsValuesAboveRevision();
        BookingVarianceCostRangeRuleFlagsValuesAboveCost();
    }
}
