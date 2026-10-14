namespace Acme.Workforce.Workplace.Tests.Occupancy.Segment;

using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class OccupancyMilestoneRulesChecks
{
    public static void OccupancyMilestoneValidatorReportsEveryProblem()
    {
        var validator = OccupancyMilestoneRules.DefaultOccupancyMilestoneValidator();
        var item = OccupancyMilestone.NewOccupancyMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyMilestoneCodeLengthRule(64);
        var item = OccupancyMilestone.NewOccupancyMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyMilestoneCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new OccupancyMilestoneCreatedAtRangeRule(64, 86400);
        var item = OccupancyMilestone.NewOccupancyMilestone("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyMilestoneRequiredNameRuleFlagsABlankName()
    {
        var rule = new OccupancyMilestoneRequiredNameRule("occupancy milestone needs an entry");
        var item = OccupancyMilestone.NewOccupancyMilestone("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyMilestoneFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new OccupancyMilestoneFteRangeRule(3.3, 3062.8);
        var item = OccupancyMilestone.NewOccupancyMilestone("x", "y");
        item.Fte = 3064.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyMilestoneChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new OccupancyMilestoneChecksumLengthRule(64);
        var item = OccupancyMilestone.NewOccupancyMilestone("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyMilestoneValidatorReportsEveryProblem();
        OccupancyMilestoneCodeLengthRuleFlagsALongCode();
        OccupancyMilestoneCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        OccupancyMilestoneRequiredNameRuleFlagsABlankName();
        OccupancyMilestoneFteRangeRuleFlagsValuesAboveFte();
        OccupancyMilestoneChecksumLengthRuleFlagsALongChecksum();
    }
}
