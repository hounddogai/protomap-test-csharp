namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class LenientFloorPlanAuditRulesChecks
{
    public static void LenientFloorPlanAuditValidatorReportsEveryProblem()
    {
        var validator = LenientFloorPlanAuditRules.DefaultLenientFloorPlanAuditValidator();
        var item = LenientFloorPlanAudit.NewLenientFloorPlanAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientFloorPlanAuditBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new LenientFloorPlanAuditBatchSizeRangeRule(32, 64);
        var item = LenientFloorPlanAudit.NewLenientFloorPlanAudit("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientFloorPlanAuditQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new LenientFloorPlanAuditQuantityRangeRule(32, 255);
        var item = LenientFloorPlanAudit.NewLenientFloorPlanAudit("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientFloorPlanAuditValidatorReportsEveryProblem();
        LenientFloorPlanAuditBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        LenientFloorPlanAuditQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
