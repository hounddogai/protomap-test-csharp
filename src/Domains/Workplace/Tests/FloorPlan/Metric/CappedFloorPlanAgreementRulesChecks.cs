namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedFloorPlanAgreementRulesChecks
{
    public static void CappedFloorPlanAgreementValidatorReportsEveryProblem()
    {
        var validator = CappedFloorPlanAgreementRules.DefaultCappedFloorPlanAgreementValidator();
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedFloorPlanAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedFloorPlanAgreementCodeLengthRule(120);
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedFloorPlanAgreementBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new CappedFloorPlanAgreementBadgeLengthRule(80);
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedFloorPlanAgreementQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new CappedFloorPlanAgreementQuantityRangeRule(80, 86400);
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedFloorPlanAgreementRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new CappedFloorPlanAgreementRatioRangeRule(0.6, 3509.0);
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("x", "y");
        item.Ratio = 3510.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedFloorPlanAgreementRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new CappedFloorPlanAgreementRequiredBadgeRule("capped floor plan agreement needs an entry");
        var item = CappedFloorPlanAgreement.NewCappedFloorPlanAgreement("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedFloorPlanAgreementValidatorReportsEveryProblem();
        CappedFloorPlanAgreementCodeLengthRuleFlagsALongCode();
        CappedFloorPlanAgreementBadgeLengthRuleFlagsALongBadge();
        CappedFloorPlanAgreementQuantityRangeRuleFlagsValuesAboveQuantity();
        CappedFloorPlanAgreementRatioRangeRuleFlagsValuesAboveRatio();
        CappedFloorPlanAgreementRequiredBadgeRuleFlagsABlankBadge();
    }
}
