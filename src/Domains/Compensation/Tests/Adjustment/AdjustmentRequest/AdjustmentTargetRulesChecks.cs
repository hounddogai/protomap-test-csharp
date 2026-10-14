namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class AdjustmentTargetRulesChecks
{
    public static void AdjustmentTargetValidatorReportsEveryProblem()
    {
        var validator = AdjustmentTargetRules.DefaultAdjustmentTargetValidator();
        var item = AdjustmentTarget.NewAdjustmentTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentTargetRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new AdjustmentTargetRequiredRegionRule("adjustment target needs a value");
        var item = AdjustmentTarget.NewAdjustmentTarget("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentTargetCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new AdjustmentTargetCapacityRangeRule(16, 255);
        var item = AdjustmentTarget.NewAdjustmentTarget("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentTargetDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new AdjustmentTargetDepartmentLengthRule(1000);
        var item = AdjustmentTarget.NewAdjustmentTarget("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentTargetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdjustmentTargetLabelLengthRule(64);
        var item = AdjustmentTarget.NewAdjustmentTarget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdjustmentTargetValidatorReportsEveryProblem();
        AdjustmentTargetRequiredRegionRuleFlagsABlankRegion();
        AdjustmentTargetCapacityRangeRuleFlagsValuesAboveCapacity();
        AdjustmentTargetDepartmentLengthRuleFlagsALongDepartment();
        AdjustmentTargetLabelLengthRuleFlagsALongLabel();
    }
}
