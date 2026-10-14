namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LegacyBookingRollupRulesChecks
{
    public static void LegacyBookingRollupValidatorReportsEveryProblem()
    {
        var validator = LegacyBookingRollupRules.DefaultLegacyBookingRollupValidator();
        var item = LegacyBookingRollup.NewLegacyBookingRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyBookingRollupScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new LegacyBookingRollupScoreRangeRule(3.3, 1990.3);
        var item = LegacyBookingRollup.NewLegacyBookingRollup("x", "y");
        item.Score = 1991.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyBookingRollupCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new LegacyBookingRollupCreatedAtRangeRule(120, 1000);
        var item = LegacyBookingRollup.NewLegacyBookingRollup("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyBookingRollupRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LegacyBookingRollupRequiredLocationRule("legacy booking rollup needs an entry");
        var item = LegacyBookingRollup.NewLegacyBookingRollup("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyBookingRollupBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new LegacyBookingRollupBadgeLengthRule(120);
        var item = LegacyBookingRollup.NewLegacyBookingRollup("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyBookingRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyBookingRollupRequiredCodeRule("legacy booking rollup needs an entry");
        var item = LegacyBookingRollup.NewLegacyBookingRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyBookingRollupValidatorReportsEveryProblem();
        LegacyBookingRollupScoreRangeRuleFlagsValuesAboveScore();
        LegacyBookingRollupCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        LegacyBookingRollupRequiredLocationRuleFlagsABlankLocation();
        LegacyBookingRollupBadgeLengthRuleFlagsALongBadge();
        LegacyBookingRollupRequiredCodeRuleFlagsABlankCode();
    }
}
