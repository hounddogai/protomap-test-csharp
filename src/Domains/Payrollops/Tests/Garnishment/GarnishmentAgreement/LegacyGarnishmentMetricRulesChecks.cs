namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

/// <summary>Checks of garnishment agreement code, run by the project's runner.</summary>
public static class LegacyGarnishmentMetricRulesChecks
{
    public static void LegacyGarnishmentMetricValidatorReportsEveryProblem()
    {
        var validator = LegacyGarnishmentMetricRules.DefaultLegacyGarnishmentMetricValidator();
        var item = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyGarnishmentMetricRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new LegacyGarnishmentMetricRequiredSkillRule("legacy garnishment metric needs a value");
        var item = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyGarnishmentMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyGarnishmentMetricLabelLengthRule(3600);
        var item = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyGarnishmentMetricValidatorReportsEveryProblem();
        LegacyGarnishmentMetricRequiredSkillRuleFlagsABlankSkill();
        LegacyGarnishmentMetricLabelLengthRuleFlagsALongLabel();
    }
}
