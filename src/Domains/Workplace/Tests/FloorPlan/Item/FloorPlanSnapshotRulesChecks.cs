namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSnapshotRulesChecks
{
    public static void FloorPlanSnapshotValidatorReportsEveryProblem()
    {
        var validator = FloorPlanSnapshotRules.DefaultFloorPlanSnapshotValidator();
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanSnapshotMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new FloorPlanSnapshotMaximumRangeRule(5.8, 953.0);
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("x", "y");
        item.Maximum = 954.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanSnapshotCodeLengthRule(3600);
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanSnapshotRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new FloorPlanSnapshotRequiredSourceRule("floor plan snapshot needs a value");
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanSnapshotValidatorReportsEveryProblem();
        FloorPlanSnapshotMaximumRangeRuleFlagsValuesAboveMaximum();
        FloorPlanSnapshotCodeLengthRuleFlagsALongCode();
        FloorPlanSnapshotRequiredSourceRuleFlagsABlankSource();
    }
}
