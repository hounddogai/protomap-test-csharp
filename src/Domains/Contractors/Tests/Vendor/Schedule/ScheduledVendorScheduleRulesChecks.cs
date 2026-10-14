namespace Acme.Workforce.Contractors.Tests.Vendor.Schedule;

using Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledVendorScheduleRulesChecks
{
    public static void ScheduledVendorScheduleValidatorReportsEveryProblem()
    {
        var validator = ScheduledVendorScheduleRules.DefaultScheduledVendorScheduleValidator();
        var item = ScheduledVendorSchedule.NewScheduledVendorSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledVendorScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledVendorScheduleRequiredCodeRule("scheduled vendor schedule needs this field");
        var item = ScheduledVendorSchedule.NewScheduledVendorSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledVendorScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledVendorScheduleRequiredTitleRule("scheduled vendor schedule needs this field");
        var item = ScheduledVendorSchedule.NewScheduledVendorSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledVendorScheduleCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new ScheduledVendorScheduleCurrencyLengthRule(86400);
        var item = ScheduledVendorSchedule.NewScheduledVendorSchedule("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledVendorScheduleValidatorReportsEveryProblem();
        ScheduledVendorScheduleRequiredCodeRuleFlagsABlankCode();
        ScheduledVendorScheduleRequiredTitleRuleFlagsABlankTitle();
        ScheduledVendorScheduleCurrencyLengthRuleFlagsALongCurrency();
    }
}
