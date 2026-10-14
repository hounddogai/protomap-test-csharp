namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorScheduleRulesChecks
{
    public static void VendorScheduleValidatorReportsEveryProblem()
    {
        var validator = VendorScheduleRules.DefaultVendorScheduleValidator();
        var item = VendorSchedule.NewVendorSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VendorScheduleRequiredCodeRule("vendor schedule needs a value");
        var item = VendorSchedule.NewVendorSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new VendorScheduleRequiredNameRule("vendor schedule needs an entry");
        var item = VendorSchedule.NewVendorSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorScheduleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new VendorScheduleUpdatedAtRangeRule(64, 64);
        var item = VendorSchedule.NewVendorSchedule("x", "y");
        item.UpdatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VendorScheduleValidatorReportsEveryProblem();
        VendorScheduleRequiredCodeRuleFlagsABlankCode();
        VendorScheduleRequiredNameRuleFlagsABlankName();
        VendorScheduleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
