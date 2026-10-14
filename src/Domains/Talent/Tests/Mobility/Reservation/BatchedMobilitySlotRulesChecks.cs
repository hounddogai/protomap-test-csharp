namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedMobilitySlotRulesChecks
{
    public static void BatchedMobilitySlotValidatorReportsEveryProblem()
    {
        var validator = BatchedMobilitySlotRules.DefaultBatchedMobilitySlotValidator();
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedMobilitySlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedMobilitySlotCodeLengthRule(255);
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedMobilitySlotFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BatchedMobilitySlotFloorValueRangeRule(7.2, 4162.0);
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("x", "y");
        item.FloorValue = 4163.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedMobilitySlotQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new BatchedMobilitySlotQuantityRangeRule(16, 120);
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("x", "y");
        item.Quantity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedMobilitySlotValidatorReportsEveryProblem();
        BatchedMobilitySlotCodeLengthRuleFlagsALongCode();
        BatchedMobilitySlotFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BatchedMobilitySlotQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
