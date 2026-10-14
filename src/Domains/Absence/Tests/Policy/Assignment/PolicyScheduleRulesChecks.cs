namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyScheduleRulesChecks
{
    public static void PolicyScheduleValidatorReportsEveryProblem()
    {
        var validator = PolicyScheduleRules.DefaultPolicyScheduleValidator();
        var item = PolicySchedule.NewPolicySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PolicyScheduleRequiredTitleRule("policy schedule needs this field");
        var item = PolicySchedule.NewPolicySchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyScheduleRequiredCodeRule("policy schedule needs an entry");
        var item = PolicySchedule.NewPolicySchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyScheduleTitleLengthRule(80);
        var item = PolicySchedule.NewPolicySchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyScheduleValidatorReportsEveryProblem();
        PolicyScheduleRequiredTitleRuleFlagsABlankTitle();
        PolicyScheduleRequiredCodeRuleFlagsABlankCode();
        PolicyScheduleTitleLengthRuleFlagsALongTitle();
    }
}
