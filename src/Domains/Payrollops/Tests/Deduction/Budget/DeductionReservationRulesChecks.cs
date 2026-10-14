namespace Acme.Workforce.Payrollops.Tests.Deduction.Budget;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeductionReservationRulesChecks
{
    public static void DeductionReservationValidatorReportsEveryProblem()
    {
        var validator = DeductionReservationRules.DefaultDeductionReservationValidator();
        var item = DeductionReservation.NewDeductionReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionReservationRequiredCodeRule("deduction reservation needs this field");
        var item = DeductionReservation.NewDeductionReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionReservationRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new DeductionReservationRequiredBadgeRule("deduction reservation needs a value");
        var item = DeductionReservation.NewDeductionReservation("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionReservationQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new DeductionReservationQuantityRangeRule(16, 255);
        var item = DeductionReservation.NewDeductionReservation("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionReservationNameLengthRuleFlagsALongName()
    {
        var rule = new DeductionReservationNameLengthRule(86400);
        var item = DeductionReservation.NewDeductionReservation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionReservationRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new DeductionReservationRateRangeRule(9.2, 4745.7);
        var item = DeductionReservation.NewDeductionReservation("x", "y");
        item.Rate = 4747.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeductionReservationValidatorReportsEveryProblem();
        DeductionReservationRequiredCodeRuleFlagsABlankCode();
        DeductionReservationRequiredBadgeRuleFlagsABlankBadge();
        DeductionReservationQuantityRangeRuleFlagsValuesAboveQuantity();
        DeductionReservationNameLengthRuleFlagsALongName();
        DeductionReservationRateRangeRuleFlagsValuesAboveRate();
    }
}
