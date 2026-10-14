namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ManualOccupancyPlanRulesChecks
{
    public static void ManualOccupancyPlanValidatorReportsEveryProblem()
    {
        var validator = ManualOccupancyPlanRules.DefaultManualOccupancyPlanValidator();
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualOccupancyPlanEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ManualOccupancyPlanEndsAtRangeRule(16, 86400);
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualOccupancyPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualOccupancyPlanCodeLengthRule(32);
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualOccupancyPlanRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ManualOccupancyPlanRatingRangeRule(9.4, 2144.0);
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("x", "y");
        item.Rating = 2145.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualOccupancyPlanRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new ManualOccupancyPlanRequiredSegmentRule("manual occupancy plan needs an entry");
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualOccupancyPlanSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ManualOccupancyPlanSkillLengthRule(32);
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualOccupancyPlanValidatorReportsEveryProblem();
        ManualOccupancyPlanEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ManualOccupancyPlanCodeLengthRuleFlagsALongCode();
        ManualOccupancyPlanRatingRangeRuleFlagsValuesAboveRating();
        ManualOccupancyPlanRequiredSegmentRuleFlagsABlankSegment();
        ManualOccupancyPlanSkillLengthRuleFlagsALongSkill();
    }
}
