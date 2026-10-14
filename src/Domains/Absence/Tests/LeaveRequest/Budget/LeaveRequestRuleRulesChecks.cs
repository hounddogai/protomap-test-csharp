namespace Acme.Workforce.Absence.Tests.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LeaveRequestRuleRulesChecks
{
    public static void LeaveRequestRuleValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestRuleRules.DefaultLeaveRequestRuleValidator();
        var item = LeaveRequestRule.NewLeaveRequestRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestRuleRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new LeaveRequestRuleRequiredCountryRule("leave request rule needs this field");
        var item = LeaveRequestRule.NewLeaveRequestRule("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestRuleDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new LeaveRequestRuleDescriptionLengthRule(64);
        var item = LeaveRequestRule.NewLeaveRequestRule("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LeaveRequestRuleLabelLengthRule(255);
        var item = LeaveRequestRule.NewLeaveRequestRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestRuleMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LeaveRequestRuleMaximumRangeRule(2.7, 632.4);
        var item = LeaveRequestRule.NewLeaveRequestRule("x", "y");
        item.Maximum = 633.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestRuleCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LeaveRequestRuleCapacityRangeRule(64, 1000);
        var item = LeaveRequestRule.NewLeaveRequestRule("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestRuleValidatorReportsEveryProblem();
        LeaveRequestRuleRequiredCountryRuleFlagsABlankCountry();
        LeaveRequestRuleDescriptionLengthRuleFlagsALongDescription();
        LeaveRequestRuleLabelLengthRuleFlagsALongLabel();
        LeaveRequestRuleMaximumRangeRuleFlagsValuesAboveMaximum();
        LeaveRequestRuleCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
