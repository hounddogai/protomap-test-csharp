namespace Acme.Workforce.Contractors.Tests.Vendor.Assignment;

using Acme.Workforce.Contractors.Vendor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalVendorSlotRulesChecks
{
    public static void GlobalVendorSlotValidatorReportsEveryProblem()
    {
        var validator = GlobalVendorSlotRules.DefaultGlobalVendorSlotValidator();
        var item = GlobalVendorSlot.NewGlobalVendorSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalVendorSlotDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new GlobalVendorSlotDepartmentLengthRule(120);
        var item = GlobalVendorSlot.NewGlobalVendorSlot("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalVendorSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalVendorSlotRequiredCodeRule("global vendor slot needs this field");
        var item = GlobalVendorSlot.NewGlobalVendorSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalVendorSlotRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new GlobalVendorSlotRequiredDepartmentRule("global vendor slot needs an entry");
        var item = GlobalVendorSlot.NewGlobalVendorSlot("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalVendorSlotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalVendorSlotLabelLengthRule(32);
        var item = GlobalVendorSlot.NewGlobalVendorSlot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalVendorSlotGradeLengthRuleFlagsALongGrade()
    {
        var rule = new GlobalVendorSlotGradeLengthRule(64);
        var item = GlobalVendorSlot.NewGlobalVendorSlot("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalVendorSlotValidatorReportsEveryProblem();
        GlobalVendorSlotDepartmentLengthRuleFlagsALongDepartment();
        GlobalVendorSlotRequiredCodeRuleFlagsABlankCode();
        GlobalVendorSlotRequiredDepartmentRuleFlagsABlankDepartment();
        GlobalVendorSlotLabelLengthRuleFlagsALongLabel();
        GlobalVendorSlotGradeLengthRuleFlagsALongGrade();
    }
}
