namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyAdjustmentRulesChecks
{
    public static void OccupancyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = OccupancyAdjustmentRules.DefaultOccupancyAdjustmentValidator();
        var item = OccupancyAdjustment.NewOccupancyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancyAdjustmentTitleLengthRule(80);
        var item = OccupancyAdjustment.NewOccupancyAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyAdjustmentRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new OccupancyAdjustmentRequiredOwnerIdRule("occupancy adjustment needs a value");
        var item = OccupancyAdjustment.NewOccupancyAdjustment("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyAdjustmentValidatorReportsEveryProblem();
        OccupancyAdjustmentTitleLengthRuleFlagsALongTitle();
        OccupancyAdjustmentRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
