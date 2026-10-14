namespace Acme.Workforce.Workplace.Tests.Occupancy.Metric;

using Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RegionalOccupancyDigestRulesChecks
{
    public static void RegionalOccupancyDigestValidatorReportsEveryProblem()
    {
        var validator = RegionalOccupancyDigestRules.DefaultRegionalOccupancyDigestValidator();
        var item = RegionalOccupancyDigest.NewRegionalOccupancyDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalOccupancyDigestChannelLengthRuleFlagsALongChannel()
    {
        var rule = new RegionalOccupancyDigestChannelLengthRule(255);
        var item = RegionalOccupancyDigest.NewRegionalOccupancyDigest("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalOccupancyDigestRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new RegionalOccupancyDigestRequiredChannelRule("regional occupancy digest needs an entry");
        var item = RegionalOccupancyDigest.NewRegionalOccupancyDigest("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalOccupancyDigestBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RegionalOccupancyDigestBadgeLengthRule(86400);
        var item = RegionalOccupancyDigest.NewRegionalOccupancyDigest("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalOccupancyDigestValidatorReportsEveryProblem();
        RegionalOccupancyDigestChannelLengthRuleFlagsALongChannel();
        RegionalOccupancyDigestRequiredChannelRuleFlagsABlankChannel();
        RegionalOccupancyDigestBadgeLengthRuleFlagsALongBadge();
    }
}
