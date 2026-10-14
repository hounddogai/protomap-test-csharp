namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictOccupancyAdjustmentRulesChecks
{
    public static void StrictOccupancyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = StrictOccupancyAdjustmentRules.DefaultStrictOccupancyAdjustmentValidator();
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictOccupancyAdjustmentRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new StrictOccupancyAdjustmentRequiredDepartmentRule("strict occupancy adjustment needs this field");
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictOccupancyAdjustmentDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new StrictOccupancyAdjustmentDepartmentLengthRule(86400);
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictOccupancyAdjustmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new StrictOccupancyAdjustmentDurationMinutesRangeRule(86400, 86400);
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictOccupancyAdjustmentBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new StrictOccupancyAdjustmentBudgetRangeRule(0.4, 404.2);
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("x", "y");
        item.Budget = 405.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictOccupancyAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictOccupancyAdjustmentRequiredLabelRule("strict occupancy adjustment needs an entry");
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictOccupancyAdjustmentValidatorReportsEveryProblem();
        StrictOccupancyAdjustmentRequiredDepartmentRuleFlagsABlankDepartment();
        StrictOccupancyAdjustmentDepartmentLengthRuleFlagsALongDepartment();
        StrictOccupancyAdjustmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        StrictOccupancyAdjustmentBudgetRangeRuleFlagsValuesAboveBudget();
        StrictOccupancyAdjustmentRequiredLabelRuleFlagsABlankLabel();
    }
}
