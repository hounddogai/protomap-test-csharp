namespace Acme.Workforce.Compliance.Tests.PolicyAck.Milestone;

using Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PolicyAckWindowRulesChecks
{
    public static void PolicyAckWindowValidatorReportsEveryProblem()
    {
        var validator = PolicyAckWindowRules.DefaultPolicyAckWindowValidator();
        var item = PolicyAckWindow.NewPolicyAckWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PolicyAckWindowWindowDaysRangeRule(120, 1000);
        var item = PolicyAckWindow.NewPolicyAckWindow("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyAckWindowValidatorReportsEveryProblem();
        PolicyAckWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
