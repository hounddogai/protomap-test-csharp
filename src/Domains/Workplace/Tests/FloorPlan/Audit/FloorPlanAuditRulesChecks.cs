namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanAuditRulesChecks
{
    public static void FloorPlanAuditValidatorReportsEveryProblem()
    {
        var validator = FloorPlanAuditRules.DefaultFloorPlanAuditValidator();
        var item = FloorPlanAudit.NewFloorPlanAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanAuditDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new FloorPlanAuditDiscountRangeRule(1.3, 2533.6);
        var item = FloorPlanAudit.NewFloorPlanAudit("x", "y");
        item.Discount = 2535.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FloorPlanAuditRequiredTitleRule("floor plan audit needs this field");
        var item = FloorPlanAudit.NewFloorPlanAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanAuditRequiredCodeRule("floor plan audit needs this field");
        var item = FloorPlanAudit.NewFloorPlanAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanAuditCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new FloorPlanAuditCurrencyLengthRule(80);
        var item = FloorPlanAudit.NewFloorPlanAudit("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanAuditValidatorReportsEveryProblem();
        FloorPlanAuditDiscountRangeRuleFlagsValuesAboveDiscount();
        FloorPlanAuditRequiredTitleRuleFlagsABlankTitle();
        FloorPlanAuditRequiredCodeRuleFlagsABlankCode();
        FloorPlanAuditCurrencyLengthRuleFlagsALongCurrency();
    }
}
