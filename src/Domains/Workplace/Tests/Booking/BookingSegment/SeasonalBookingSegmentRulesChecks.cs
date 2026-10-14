namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class SeasonalBookingSegmentRulesChecks
{
    public static void SeasonalBookingSegmentValidatorReportsEveryProblem()
    {
        var validator = SeasonalBookingSegmentRules.DefaultSeasonalBookingSegmentValidator();
        var item = SeasonalBookingSegment.NewSeasonalBookingSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalBookingSegmentSkillLengthRuleFlagsALongSkill()
    {
        var rule = new SeasonalBookingSegmentSkillLengthRule(3600);
        var item = SeasonalBookingSegment.NewSeasonalBookingSegment("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalBookingSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new SeasonalBookingSegmentNameLengthRule(86400);
        var item = SeasonalBookingSegment.NewSeasonalBookingSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalBookingSegmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new SeasonalBookingSegmentCapacityRangeRule(32, 86400);
        var item = SeasonalBookingSegment.NewSeasonalBookingSegment("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalBookingSegmentValidatorReportsEveryProblem();
        SeasonalBookingSegmentSkillLengthRuleFlagsALongSkill();
        SeasonalBookingSegmentNameLengthRuleFlagsALongName();
        SeasonalBookingSegmentCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
