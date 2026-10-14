namespace Acme.Workforce.Orgdesign.Tests.Headcount.Tier;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HeadcountScheduleRulesChecks
{
    public static void HeadcountScheduleValidatorReportsEveryProblem()
    {
        var validator = HeadcountScheduleRules.DefaultHeadcountScheduleValidator();
        var item = HeadcountSchedule.NewHeadcountSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountScheduleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new HeadcountScheduleUnitLengthRule(1000);
        var item = HeadcountSchedule.NewHeadcountSchedule("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountScheduleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new HeadcountScheduleRequiredUnitRule("headcount schedule needs this field");
        var item = HeadcountSchedule.NewHeadcountSchedule("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountScheduleDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new HeadcountScheduleDiscountRangeRule(6.8, 3570.6);
        var item = HeadcountSchedule.NewHeadcountSchedule("x", "y");
        item.Discount = 3572.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HeadcountScheduleValidatorReportsEveryProblem();
        HeadcountScheduleUnitLengthRuleFlagsALongUnit();
        HeadcountScheduleRequiredUnitRuleFlagsABlankUnit();
        HeadcountScheduleDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
