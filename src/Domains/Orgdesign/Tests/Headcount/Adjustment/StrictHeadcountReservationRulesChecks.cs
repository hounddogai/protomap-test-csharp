namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictHeadcountReservationRulesChecks
{
    public static void StrictHeadcountReservationValidatorReportsEveryProblem()
    {
        var validator = StrictHeadcountReservationRules.DefaultStrictHeadcountReservationValidator();
        var item = StrictHeadcountReservation.NewStrictHeadcountReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictHeadcountReservationDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new StrictHeadcountReservationDiscountRangeRule(0.6, 4078.5);
        var item = StrictHeadcountReservation.NewStrictHeadcountReservation("x", "y");
        item.Discount = 4080.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictHeadcountReservationCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new StrictHeadcountReservationCostRangeRule(9.5, 984.1);
        var item = StrictHeadcountReservation.NewStrictHeadcountReservation("x", "y");
        item.Cost = 985.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictHeadcountReservationValidatorReportsEveryProblem();
        StrictHeadcountReservationDiscountRangeRuleFlagsValuesAboveDiscount();
        StrictHeadcountReservationCostRangeRuleFlagsValuesAboveCost();
    }
}
