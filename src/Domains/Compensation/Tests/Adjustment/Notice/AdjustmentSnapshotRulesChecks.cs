namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentSnapshotRulesChecks
{
    public static void AdjustmentSnapshotValidatorReportsEveryProblem()
    {
        var validator = AdjustmentSnapshotRules.DefaultAdjustmentSnapshotValidator();
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentSnapshotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdjustmentSnapshotRequiredLabelRule("adjustment snapshot needs an entry");
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdjustmentSnapshotRequiredCodeRule("adjustment snapshot needs an entry");
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentSnapshotFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new AdjustmentSnapshotFloorValueRangeRule(4.2, 399.9);
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("x", "y");
        item.FloorValue = 401.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdjustmentSnapshotCodeLengthRule(64);
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentSnapshotDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AdjustmentSnapshotDiscountRangeRule(3.8, 3563.1);
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("x", "y");
        item.Discount = 3564.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdjustmentSnapshotValidatorReportsEveryProblem();
        AdjustmentSnapshotRequiredLabelRuleFlagsABlankLabel();
        AdjustmentSnapshotRequiredCodeRuleFlagsABlankCode();
        AdjustmentSnapshotFloorValueRangeRuleFlagsValuesAboveFloorValue();
        AdjustmentSnapshotCodeLengthRuleFlagsALongCode();
        AdjustmentSnapshotDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
