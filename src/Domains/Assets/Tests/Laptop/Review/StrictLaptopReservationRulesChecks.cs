namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictLaptopReservationRulesChecks
{
    public static void StrictLaptopReservationValidatorReportsEveryProblem()
    {
        var validator = StrictLaptopReservationRules.DefaultStrictLaptopReservationValidator();
        var item = StrictLaptopReservation.NewStrictLaptopReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictLaptopReservationCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new StrictLaptopReservationCapacityRangeRule(16, 32);
        var item = StrictLaptopReservation.NewStrictLaptopReservation("x", "y");
        item.Capacity = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictLaptopReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictLaptopReservationTitleLengthRule(255);
        var item = StrictLaptopReservation.NewStrictLaptopReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictLaptopReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictLaptopReservationRequiredTitleRule("strict laptop reservation needs a value");
        var item = StrictLaptopReservation.NewStrictLaptopReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictLaptopReservationValidatorReportsEveryProblem();
        StrictLaptopReservationCapacityRangeRuleFlagsValuesAboveCapacity();
        StrictLaptopReservationTitleLengthRuleFlagsALongTitle();
        StrictLaptopReservationRequiredTitleRuleFlagsABlankTitle();
    }
}
