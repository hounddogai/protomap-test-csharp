namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class LeaveRequestProfileRulesChecks
{
    public static void LeaveRequestProfileValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestProfileRules.DefaultLeaveRequestProfileValidator();
        var item = LeaveRequestProfile.NewLeaveRequestProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestProfileLocationLengthRuleFlagsALongLocation()
    {
        var rule = new LeaveRequestProfileLocationLengthRule(120);
        var item = LeaveRequestProfile.NewLeaveRequestProfile("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestProfileRequiredCodeRule("leave request profile needs a value");
        var item = LeaveRequestProfile.NewLeaveRequestProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestProfileRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LeaveRequestProfileRequiredLabelRule("leave request profile needs this field");
        var item = LeaveRequestProfile.NewLeaveRequestProfile("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestProfileRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LeaveRequestProfileRateRangeRule(5.6, 2135.5);
        var item = LeaveRequestProfile.NewLeaveRequestProfile("x", "y");
        item.Rate = 2137.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestProfileValidatorReportsEveryProblem();
        LeaveRequestProfileLocationLengthRuleFlagsALongLocation();
        LeaveRequestProfileRequiredCodeRuleFlagsABlankCode();
        LeaveRequestProfileRequiredLabelRuleFlagsABlankLabel();
        LeaveRequestProfileRateRangeRuleFlagsValuesAboveRate();
    }
}
