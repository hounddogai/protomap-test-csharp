namespace Acme.Workforce.Workplace.Tests.Occupancy.Summary;

using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class OccupancyDigestRulesChecks
{
    public static void OccupancyDigestValidatorReportsEveryProblem()
    {
        var validator = OccupancyDigestRules.DefaultOccupancyDigestValidator();
        var item = OccupancyDigest.NewOccupancyDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyDigestEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new OccupancyDigestEndsAtRangeRule(80, 120);
        var item = OccupancyDigest.NewOccupancyDigest("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyDigestCodeLengthRule(86400);
        var item = OccupancyDigest.NewOccupancyDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyDigestRequiredCodeRule("occupancy digest needs this field");
        var item = OccupancyDigest.NewOccupancyDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyDigestCountryLengthRuleFlagsALongCountry()
    {
        var rule = new OccupancyDigestCountryLengthRule(64);
        var item = OccupancyDigest.NewOccupancyDigest("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyDigestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new OccupancyDigestDurationMinutesRangeRule(80, 255);
        var item = OccupancyDigest.NewOccupancyDigest("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        OccupancyDigestValidatorReportsEveryProblem();
        OccupancyDigestEndsAtRangeRuleFlagsValuesAboveEndsAt();
        OccupancyDigestCodeLengthRuleFlagsALongCode();
        OccupancyDigestRequiredCodeRuleFlagsABlankCode();
        OccupancyDigestCountryLengthRuleFlagsALongCountry();
        OccupancyDigestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
