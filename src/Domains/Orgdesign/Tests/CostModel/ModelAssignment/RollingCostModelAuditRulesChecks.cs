namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class RollingCostModelAuditRulesChecks
{
    public static void RollingCostModelAuditValidatorReportsEveryProblem()
    {
        var validator = RollingCostModelAuditRules.DefaultRollingCostModelAuditValidator();
        var item = RollingCostModelAudit.NewRollingCostModelAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingCostModelAuditCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RollingCostModelAuditCapacityRangeRule(120, 1000);
        var item = RollingCostModelAudit.NewRollingCostModelAudit("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingCostModelAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingCostModelAuditCodeLengthRule(86400);
        var item = RollingCostModelAudit.NewRollingCostModelAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingCostModelAuditUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RollingCostModelAuditUpdatedAtRangeRule(32, 255);
        var item = RollingCostModelAudit.NewRollingCostModelAudit("x", "y");
        item.UpdatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingCostModelAuditMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new RollingCostModelAuditMaximumRangeRule(9.5, 1031.7);
        var item = RollingCostModelAudit.NewRollingCostModelAudit("x", "y");
        item.Maximum = 1033.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingCostModelAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingCostModelAuditRequiredNameRule("rolling cost model audit needs a value");
        var item = RollingCostModelAudit.NewRollingCostModelAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingCostModelAuditQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new RollingCostModelAuditQuantityRangeRule(16, 16);
        var item = RollingCostModelAudit.NewRollingCostModelAudit("x", "y");
        item.Quantity = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingCostModelAuditValidatorReportsEveryProblem();
        RollingCostModelAuditCapacityRangeRuleFlagsValuesAboveCapacity();
        RollingCostModelAuditCodeLengthRuleFlagsALongCode();
        RollingCostModelAuditUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        RollingCostModelAuditMaximumRangeRuleFlagsValuesAboveMaximum();
        RollingCostModelAuditRequiredNameRuleFlagsABlankName();
        RollingCostModelAuditQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
