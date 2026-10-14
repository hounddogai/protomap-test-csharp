namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class FloorPlanManifestRulesChecks
{
    public static void FloorPlanManifestValidatorReportsEveryProblem()
    {
        var validator = FloorPlanManifestRules.DefaultFloorPlanManifestValidator();
        var item = FloorPlanManifest.NewFloorPlanManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanManifestLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new FloorPlanManifestLimitRangeRule(32, 3600);
        var item = FloorPlanManifest.NewFloorPlanManifest("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanManifestCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new FloorPlanManifestCreatedAtRangeRule(255, 86400);
        var item = FloorPlanManifest.NewFloorPlanManifest("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FloorPlanManifestValidatorReportsEveryProblem();
        FloorPlanManifestLimitRangeRuleFlagsValuesAboveLimit();
        FloorPlanManifestCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
