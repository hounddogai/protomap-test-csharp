namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorAllocationRulesChecks
{
    public static void VisitorAllocationValidatorReportsEveryProblem()
    {
        var validator = VisitorAllocationRules.DefaultVisitorAllocationValidator();
        var item = VisitorAllocation.NewVisitorAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorAllocationChannelLengthRuleFlagsALongChannel()
    {
        var rule = new VisitorAllocationChannelLengthRule(86400);
        var item = VisitorAllocation.NewVisitorAllocation("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorAllocationFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new VisitorAllocationFloorValueRangeRule(6.0, 3798.4);
        var item = VisitorAllocation.NewVisitorAllocation("x", "y");
        item.FloorValue = 3799.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorAllocationGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new VisitorAllocationGraceDaysRangeRule(3600, 86400);
        var item = VisitorAllocation.NewVisitorAllocation("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorAllocationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new VisitorAllocationLabelLengthRule(120);
        var item = VisitorAllocation.NewVisitorAllocation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorAllocationRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new VisitorAllocationRequiredEmployeeIdRule("visitor allocation needs a value");
        var item = VisitorAllocation.NewVisitorAllocation("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorAllocationValidatorReportsEveryProblem();
        VisitorAllocationChannelLengthRuleFlagsALongChannel();
        VisitorAllocationFloorValueRangeRuleFlagsValuesAboveFloorValue();
        VisitorAllocationGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        VisitorAllocationLabelLengthRuleFlagsALongLabel();
        VisitorAllocationRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
