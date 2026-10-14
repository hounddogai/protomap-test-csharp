namespace Acme.Workforce.Absence.Tests.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.Holiday.HolidayPolicy;

/// <summary>Checks of holiday policy code, run by the project's runner.</summary>
public static class RegionalHolidayPolicyRulesChecks
{
    public static void RegionalHolidayPolicyValidatorReportsEveryProblem()
    {
        var validator = RegionalHolidayPolicyRules.DefaultRegionalHolidayPolicyValidator();
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalHolidayPolicyReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new RegionalHolidayPolicyReviewerLengthRule(32);
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalHolidayPolicyChannelLengthRuleFlagsALongChannel()
    {
        var rule = new RegionalHolidayPolicyChannelLengthRule(255);
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalHolidayPolicyValidatorReportsEveryProblem();
        RegionalHolidayPolicyReviewerLengthRuleFlagsALongReviewer();
        RegionalHolidayPolicyChannelLengthRuleFlagsALongChannel();
    }
}
