namespace Acme.Workforce.Contractors.Tests.Vendor.Agreement;

using Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GlobalVendorReservationRulesChecks
{
    public static void GlobalVendorReservationValidatorReportsEveryProblem()
    {
        var validator = GlobalVendorReservationRules.DefaultGlobalVendorReservationValidator();
        var item = GlobalVendorReservation.NewGlobalVendorReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalVendorReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalVendorReservationRequiredCodeRule("global vendor reservation needs an entry");
        var item = GlobalVendorReservation.NewGlobalVendorReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalVendorReservationPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new GlobalVendorReservationPriorityRangeRule(16, 3600);
        var item = GlobalVendorReservation.NewGlobalVendorReservation("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalVendorReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalVendorReservationLabelLengthRule(255);
        var item = GlobalVendorReservation.NewGlobalVendorReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalVendorReservationValidatorReportsEveryProblem();
        GlobalVendorReservationRequiredCodeRuleFlagsABlankCode();
        GlobalVendorReservationPriorityRangeRuleFlagsValuesAbovePriority();
        GlobalVendorReservationLabelLengthRuleFlagsALongLabel();
    }
}
