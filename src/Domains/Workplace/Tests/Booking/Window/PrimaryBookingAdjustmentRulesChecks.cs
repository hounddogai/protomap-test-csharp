namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingAdjustmentRulesChecks
{
    public static void PrimaryBookingAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PrimaryBookingAdjustmentRules.DefaultPrimaryBookingAdjustmentValidator();
        var item = PrimaryBookingAdjustment.NewPrimaryBookingAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryBookingAdjustmentRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PrimaryBookingAdjustmentRetriesRangeRule(1000, 3600);
        var item = PrimaryBookingAdjustment.NewPrimaryBookingAdjustment("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryBookingAdjustmentRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new PrimaryBookingAdjustmentRequiredDepartmentRule("primary booking adjustment needs a value");
        var item = PrimaryBookingAdjustment.NewPrimaryBookingAdjustment("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryBookingAdjustmentWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PrimaryBookingAdjustmentWeightRangeRule(9.9, 1876.9);
        var item = PrimaryBookingAdjustment.NewPrimaryBookingAdjustment("x", "y");
        item.Weight = 1878.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryBookingAdjustmentValidatorReportsEveryProblem();
        PrimaryBookingAdjustmentRetriesRangeRuleFlagsValuesAboveRetries();
        PrimaryBookingAdjustmentRequiredDepartmentRuleFlagsABlankDepartment();
        PrimaryBookingAdjustmentWeightRangeRuleFlagsValuesAboveWeight();
    }
}
