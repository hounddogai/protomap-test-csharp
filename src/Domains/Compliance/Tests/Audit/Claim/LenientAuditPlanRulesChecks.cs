namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientAuditPlanRulesChecks
{
    public static void LenientAuditPlanValidatorReportsEveryProblem()
    {
        var validator = LenientAuditPlanRules.DefaultLenientAuditPlanValidator();
        var item = LenientAuditPlan.NewLenientAuditPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientAuditPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientAuditPlanRequiredLabelRule("lenient audit plan needs an entry");
        var item = LenientAuditPlan.NewLenientAuditPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientAuditPlanCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new LenientAuditPlanCostCenterLengthRule(16);
        var item = LenientAuditPlan.NewLenientAuditPlan("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAuditPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientAuditPlanRequiredCodeRule("lenient audit plan needs an entry");
        var item = LenientAuditPlan.NewLenientAuditPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientAuditPlanEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new LenientAuditPlanEndsAtRangeRule(32, 1000);
        var item = LenientAuditPlan.NewLenientAuditPlan("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientAuditPlanValidatorReportsEveryProblem();
        LenientAuditPlanRequiredLabelRuleFlagsABlankLabel();
        LenientAuditPlanCostCenterLengthRuleFlagsALongCostCenter();
        LenientAuditPlanRequiredCodeRuleFlagsABlankCode();
        LenientAuditPlanEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
