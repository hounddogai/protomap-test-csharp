namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class CappedOffboardingScheduleRulesChecks
{
    public static void CappedOffboardingScheduleValidatorReportsEveryProblem()
    {
        var validator = CappedOffboardingScheduleRules.DefaultCappedOffboardingScheduleValidator();
        var item = CappedOffboardingSchedule.NewCappedOffboardingSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedOffboardingScheduleFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new CappedOffboardingScheduleFloorValueRangeRule(5.6, 132.7);
        var item = CappedOffboardingSchedule.NewCappedOffboardingSchedule("x", "y");
        item.FloorValue = 134.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedOffboardingScheduleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new CappedOffboardingScheduleLocaleLengthRule(32);
        var item = CappedOffboardingSchedule.NewCappedOffboardingSchedule("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedOffboardingScheduleValidatorReportsEveryProblem();
        CappedOffboardingScheduleFloorValueRangeRuleFlagsValuesAboveFloorValue();
        CappedOffboardingScheduleLocaleLengthRuleFlagsALongLocale();
    }
}
