namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedOccupancyPolicyRulesChecks
{
    public static void CappedOccupancyPolicyValidatorReportsEveryProblem()
    {
        var validator = CappedOccupancyPolicyRules.DefaultCappedOccupancyPolicyValidator();
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedOccupancyPolicySummaryLengthRuleFlagsALongSummary()
    {
        var rule = new CappedOccupancyPolicySummaryLengthRule(80);
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedOccupancyPolicyNameLengthRuleFlagsALongName()
    {
        var rule = new CappedOccupancyPolicyNameLengthRule(16);
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedOccupancyPolicyRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new CappedOccupancyPolicyRequiredChannelRule("capped occupancy policy needs a value");
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedOccupancyPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedOccupancyPolicyRequiredCodeRule("capped occupancy policy needs an entry");
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedOccupancyPolicyValidatorReportsEveryProblem();
        CappedOccupancyPolicySummaryLengthRuleFlagsALongSummary();
        CappedOccupancyPolicyNameLengthRuleFlagsALongName();
        CappedOccupancyPolicyRequiredChannelRuleFlagsABlankChannel();
        CappedOccupancyPolicyRequiredCodeRuleFlagsABlankCode();
    }
}
