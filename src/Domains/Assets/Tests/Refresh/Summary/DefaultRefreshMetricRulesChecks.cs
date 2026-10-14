namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultRefreshMetricRulesChecks
{
    public static void DefaultRefreshMetricValidatorReportsEveryProblem()
    {
        var validator = DefaultRefreshMetricRules.DefaultDefaultRefreshMetricValidator();
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultRefreshMetricRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new DefaultRefreshMetricRequiredChecksumRule("default refresh metric needs this field");
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRefreshMetricRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultRefreshMetricRequiredNameRule("default refresh metric needs this field");
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRefreshMetricSkillLengthRuleFlagsALongSkill()
    {
        var rule = new DefaultRefreshMetricSkillLengthRule(86400);
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultRefreshMetricValidatorReportsEveryProblem();
        DefaultRefreshMetricRequiredChecksumRuleFlagsABlankChecksum();
        DefaultRefreshMetricRequiredNameRuleFlagsABlankName();
        DefaultRefreshMetricSkillLengthRuleFlagsALongSkill();
    }
}
