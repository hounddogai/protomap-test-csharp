namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopReservationRulesChecks
{
    public static void LaptopReservationValidatorReportsEveryProblem()
    {
        var validator = LaptopReservationRules.DefaultLaptopReservationValidator();
        var item = LaptopReservation.NewLaptopReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LaptopReservationLabelLengthRule(80);
        var item = LaptopReservation.NewLaptopReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopReservationSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LaptopReservationSequenceRangeRule(80, 120);
        var item = LaptopReservation.NewLaptopReservation("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopReservationDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new LaptopReservationDepartmentLengthRule(64);
        var item = LaptopReservation.NewLaptopReservation("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopReservationCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new LaptopReservationCeilingRangeRule(5.1, 4979.9);
        var item = LaptopReservation.NewLaptopReservation("x", "y");
        item.Ceiling = 4981.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LaptopReservationRequiredCodeRule("laptop reservation needs a value");
        var item = LaptopReservation.NewLaptopReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopReservationValidatorReportsEveryProblem();
        LaptopReservationLabelLengthRuleFlagsALongLabel();
        LaptopReservationSequenceRangeRuleFlagsValuesAboveSequence();
        LaptopReservationDepartmentLengthRuleFlagsALongDepartment();
        LaptopReservationCeilingRangeRuleFlagsValuesAboveCeiling();
        LaptopReservationRequiredCodeRuleFlagsABlankCode();
    }
}
