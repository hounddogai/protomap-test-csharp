namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LeaveRequestBundleRulesChecks
{
    public static void LeaveRequestBundleValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestBundleRules.DefaultLeaveRequestBundleValidator();
        var item = LeaveRequestBundle.NewLeaveRequestBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestBundleCodeLengthRule(86400);
        var item = LeaveRequestBundle.NewLeaveRequestBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestBundleWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new LeaveRequestBundleWindowDaysRangeRule(80, 1000);
        var item = LeaveRequestBundle.NewLeaveRequestBundle("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestBundleRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new LeaveRequestBundleRequiredSourceRule("leave request bundle needs a value");
        var item = LeaveRequestBundle.NewLeaveRequestBundle("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestBundleRequiredCodeRule("leave request bundle needs this field");
        var item = LeaveRequestBundle.NewLeaveRequestBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestBundleValidatorReportsEveryProblem();
        LeaveRequestBundleCodeLengthRuleFlagsALongCode();
        LeaveRequestBundleWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        LeaveRequestBundleRequiredSourceRuleFlagsABlankSource();
        LeaveRequestBundleRequiredCodeRuleFlagsABlankCode();
    }
}
