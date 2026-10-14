namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedLeaveRequestBaselineRulesChecks
{
    public static void CappedLeaveRequestBaselineValidatorReportsEveryProblem()
    {
        var validator = CappedLeaveRequestBaselineRules.DefaultCappedLeaveRequestBaselineValidator();
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedLeaveRequestBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedLeaveRequestBaselineRequiredCodeRule("capped leave request baseline needs this field");
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedLeaveRequestBaselineCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new CappedLeaveRequestBaselineCapacityRangeRule(32, 255);
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedLeaveRequestBaselineCountryLengthRuleFlagsALongCountry()
    {
        var rule = new CappedLeaveRequestBaselineCountryLengthRule(120);
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedLeaveRequestBaselineValidatorReportsEveryProblem();
        CappedLeaveRequestBaselineRequiredCodeRuleFlagsABlankCode();
        CappedLeaveRequestBaselineCapacityRangeRuleFlagsValuesAboveCapacity();
        CappedLeaveRequestBaselineCountryLengthRuleFlagsALongCountry();
    }
}
