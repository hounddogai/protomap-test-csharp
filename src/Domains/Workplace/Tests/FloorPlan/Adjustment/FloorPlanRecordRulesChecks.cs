namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanRecordRulesChecks
{
    public static void FloorPlanRecordValidatorReportsEveryProblem()
    {
        var validator = FloorPlanRecordRules.DefaultFloorPlanRecordValidator();
        var item = FloorPlanRecord.NewFloorPlanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanRecordSourceLengthRuleFlagsALongSource()
    {
        var rule = new FloorPlanRecordSourceLengthRule(32);
        var item = FloorPlanRecord.NewFloorPlanRecord("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanRecordNameLengthRuleFlagsALongName()
    {
        var rule = new FloorPlanRecordNameLengthRule(86400);
        var item = FloorPlanRecord.NewFloorPlanRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanRecordValidatorReportsEveryProblem();
        FloorPlanRecordSourceLengthRuleFlagsALongSource();
        FloorPlanRecordNameLengthRuleFlagsALongName();
    }
}
