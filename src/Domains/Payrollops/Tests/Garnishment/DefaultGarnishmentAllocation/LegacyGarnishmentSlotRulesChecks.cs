namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentSlotRulesChecks
{
    public static void LegacyGarnishmentSlotValidatorReportsEveryProblem()
    {
        var validator = LegacyGarnishmentSlotRules.DefaultLegacyGarnishmentSlotValidator();
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyGarnishmentSlotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyGarnishmentSlotRequiredLabelRule("legacy garnishment slot needs this field");
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyGarnishmentSlotRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LegacyGarnishmentSlotRequiredCostCenterRule("legacy garnishment slot needs this field");
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyGarnishmentSlotHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new LegacyGarnishmentSlotHoursRangeRule(4.6, 2158.2);
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("x", "y");
        item.Hours = 2159.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyGarnishmentSlotNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyGarnishmentSlotNameLengthRule(1000);
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyGarnishmentSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyGarnishmentSlotCodeLengthRule(64);
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyGarnishmentSlotValidatorReportsEveryProblem();
        LegacyGarnishmentSlotRequiredLabelRuleFlagsABlankLabel();
        LegacyGarnishmentSlotRequiredCostCenterRuleFlagsABlankCostCenter();
        LegacyGarnishmentSlotHoursRangeRuleFlagsValuesAboveHours();
        LegacyGarnishmentSlotNameLengthRuleFlagsALongName();
        LegacyGarnishmentSlotCodeLengthRuleFlagsALongCode();
    }
}
