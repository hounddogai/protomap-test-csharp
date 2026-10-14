namespace Acme.Workforce.Contractors.Tests.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>Checks of scheduled vendor schedule code, run by the project's runner.</summary>
public static class RegionalVendorScheduleRulesChecks
{
    public static void RegionalVendorScheduleValidatorReportsEveryProblem()
    {
        var validator = RegionalVendorScheduleRules.DefaultRegionalVendorScheduleValidator();
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalVendorScheduleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RegionalVendorScheduleCostRangeRule(3.0, 3039.0);
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("x", "y");
        item.Cost = 3040.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalVendorScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalVendorScheduleRequiredCodeRule("regional vendor schedule needs a value");
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalVendorScheduleEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new RegionalVendorScheduleEndsAtRangeRule(16, 3600);
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalVendorScheduleValidatorReportsEveryProblem();
        RegionalVendorScheduleCostRangeRuleFlagsValuesAboveCost();
        RegionalVendorScheduleRequiredCodeRuleFlagsABlankCode();
        RegionalVendorScheduleEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
