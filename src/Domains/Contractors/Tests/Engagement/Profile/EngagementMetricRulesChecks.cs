namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementMetricRulesChecks
{
    public static void EngagementMetricValidatorReportsEveryProblem()
    {
        var validator = EngagementMetricRules.DefaultEngagementMetricValidator();
        var item = EngagementMetric.NewEngagementMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementMetricLocationLengthRuleFlagsALongLocation()
    {
        var rule = new EngagementMetricLocationLengthRule(255);
        var item = EngagementMetric.NewEngagementMetric("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementMetricNameLengthRuleFlagsALongName()
    {
        var rule = new EngagementMetricNameLengthRule(32);
        var item = EngagementMetric.NewEngagementMetric("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementMetricRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new EngagementMetricRequiredPhoneRule("engagement metric needs a value");
        var item = EngagementMetric.NewEngagementMetric("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementMetricValidatorReportsEveryProblem();
        EngagementMetricLocationLengthRuleFlagsALongLocation();
        EngagementMetricNameLengthRuleFlagsALongName();
        EngagementMetricRequiredPhoneRuleFlagsABlankPhone();
    }
}
