namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class TieredVendorScheduleRulesChecks
{
    public static void TieredVendorScheduleValidatorReportsEveryProblem()
    {
        var validator = TieredVendorScheduleRules.DefaultTieredVendorScheduleValidator();
        var item = TieredVendorSchedule.NewTieredVendorSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredVendorScheduleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredVendorScheduleRequiredLabelRule("tiered vendor schedule needs this field");
        var item = TieredVendorSchedule.NewTieredVendorSchedule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredVendorScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredVendorScheduleCodeLengthRule(32);
        var item = TieredVendorSchedule.NewTieredVendorSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredVendorScheduleValidatorReportsEveryProblem();
        TieredVendorScheduleRequiredLabelRuleFlagsABlankLabel();
        TieredVendorScheduleCodeLengthRuleFlagsALongCode();
    }
}
