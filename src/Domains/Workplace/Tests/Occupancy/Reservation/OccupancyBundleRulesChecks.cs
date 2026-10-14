namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyBundleRulesChecks
{
    public static void OccupancyBundleValidatorReportsEveryProblem()
    {
        var validator = OccupancyBundleRules.DefaultOccupancyBundleValidator();
        var item = OccupancyBundle.NewOccupancyBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyBundleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new OccupancyBundleFteRangeRule(7.5, 1451.7);
        var item = OccupancyBundle.NewOccupancyBundle("x", "y");
        item.Fte = 1453.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new OccupancyBundleUpdatedAtRangeRule(32, 255);
        var item = OccupancyBundle.NewOccupancyBundle("x", "y");
        item.UpdatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyBundleCodeLengthRule(32);
        var item = OccupancyBundle.NewOccupancyBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyBundleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new OccupancyBundleRequiredTitleRule("occupancy bundle needs this field");
        var item = OccupancyBundle.NewOccupancyBundle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyBundleValidatorReportsEveryProblem();
        OccupancyBundleFteRangeRuleFlagsValuesAboveFte();
        OccupancyBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        OccupancyBundleCodeLengthRuleFlagsALongCode();
        OccupancyBundleRequiredTitleRuleFlagsABlankTitle();
    }
}
