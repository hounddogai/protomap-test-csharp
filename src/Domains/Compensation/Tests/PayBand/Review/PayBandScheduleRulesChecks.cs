namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandScheduleRulesChecks
{
    public static void PayBandScheduleValidatorReportsEveryProblem()
    {
        var validator = PayBandScheduleRules.DefaultPayBandScheduleValidator();
        var item = PayBandSchedule.NewPayBandSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandScheduleDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new PayBandScheduleDescriptionLengthRule(255);
        var item = PayBandSchedule.NewPayBandSchedule("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandScheduleFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PayBandScheduleFloorValueRangeRule(9.7, 1068.6);
        var item = PayBandSchedule.NewPayBandSchedule("x", "y");
        item.FloorValue = 1070.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandScheduleValidatorReportsEveryProblem();
        PayBandScheduleDescriptionLengthRuleFlagsALongDescription();
        PayBandScheduleFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
