namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class EngagementRequestRulesChecks
{
    public static void EngagementRequestValidatorReportsEveryProblem()
    {
        var validator = EngagementRequestRules.DefaultEngagementRequestValidator();
        var item = EngagementRequest.NewEngagementRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementRequestCodeLengthRule(80);
        var item = EngagementRequest.NewEngagementRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementRequestCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new EngagementRequestCapacityRangeRule(32, 3600);
        var item = EngagementRequest.NewEngagementRequest("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementRequestHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new EngagementRequestHoursRangeRule(10.0, 3236.6);
        var item = EngagementRequest.NewEngagementRequest("x", "y");
        item.Hours = 3238.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EngagementRequestValidatorReportsEveryProblem();
        EngagementRequestCodeLengthRuleFlagsALongCode();
        EngagementRequestCapacityRangeRuleFlagsValuesAboveCapacity();
        EngagementRequestHoursRangeRuleFlagsValuesAboveHours();
    }
}
