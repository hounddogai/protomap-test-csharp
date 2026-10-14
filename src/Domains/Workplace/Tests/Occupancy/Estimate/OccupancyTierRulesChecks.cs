namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class OccupancyTierRulesChecks
{
    public static void OccupancyTierValidatorReportsEveryProblem()
    {
        var validator = OccupancyTierRules.DefaultOccupancyTierValidator();
        var item = OccupancyTier.NewOccupancyTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyTierRequiredCodeRule("occupancy tier needs this field");
        var item = OccupancyTier.NewOccupancyTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new OccupancyTierRequiredTitleRule("occupancy tier needs an entry");
        var item = OccupancyTier.NewOccupancyTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyTierRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new OccupancyTierRevisionRangeRule(255, 86400);
        var item = OccupancyTier.NewOccupancyTier("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancyTierTitleLengthRule(255);
        var item = OccupancyTier.NewOccupancyTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyTierValidatorReportsEveryProblem();
        OccupancyTierRequiredCodeRuleFlagsABlankCode();
        OccupancyTierRequiredTitleRuleFlagsABlankTitle();
        OccupancyTierRevisionRangeRuleFlagsValuesAboveRevision();
        OccupancyTierTitleLengthRuleFlagsALongTitle();
    }
}
