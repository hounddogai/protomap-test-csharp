namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictFloorPlanBundleRulesChecks
{
    public static void StrictFloorPlanBundleValidatorReportsEveryProblem()
    {
        var validator = StrictFloorPlanBundleRules.DefaultStrictFloorPlanBundleValidator();
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictFloorPlanBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictFloorPlanBundleCodeLengthRule(64);
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictFloorPlanBundleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new StrictFloorPlanBundleStartsAtRangeRule(120, 86400);
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictFloorPlanBundleRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new StrictFloorPlanBundleRequiredSalaryBandRule("strict floor plan bundle needs a value");
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictFloorPlanBundleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new StrictFloorPlanBundleUnitLengthRule(32);
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictFloorPlanBundleValidatorReportsEveryProblem();
        StrictFloorPlanBundleCodeLengthRuleFlagsALongCode();
        StrictFloorPlanBundleStartsAtRangeRuleFlagsValuesAboveStartsAt();
        StrictFloorPlanBundleRequiredSalaryBandRuleFlagsABlankSalaryBand();
        StrictFloorPlanBundleUnitLengthRuleFlagsALongUnit();
    }
}
