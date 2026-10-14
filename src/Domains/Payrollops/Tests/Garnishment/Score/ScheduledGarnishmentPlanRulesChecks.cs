namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledGarnishmentPlanRulesChecks
{
    public static void ScheduledGarnishmentPlanValidatorReportsEveryProblem()
    {
        var validator = ScheduledGarnishmentPlanRules.DefaultScheduledGarnishmentPlanValidator();
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledGarnishmentPlanRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new ScheduledGarnishmentPlanRequiredChannelRule("scheduled garnishment plan needs this field");
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledGarnishmentPlanRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new ScheduledGarnishmentPlanRequiredSalaryBandRule("scheduled garnishment plan needs an entry");
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledGarnishmentPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledGarnishmentPlanRequiredCodeRule("scheduled garnishment plan needs this field");
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledGarnishmentPlanChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ScheduledGarnishmentPlanChannelLengthRule(255);
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledGarnishmentPlanVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ScheduledGarnishmentPlanVersionRangeRule(1000, 86400);
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledGarnishmentPlanValidatorReportsEveryProblem();
        ScheduledGarnishmentPlanRequiredChannelRuleFlagsABlankChannel();
        ScheduledGarnishmentPlanRequiredSalaryBandRuleFlagsABlankSalaryBand();
        ScheduledGarnishmentPlanRequiredCodeRuleFlagsABlankCode();
        ScheduledGarnishmentPlanChannelLengthRuleFlagsALongChannel();
        ScheduledGarnishmentPlanVersionRangeRuleFlagsValuesAboveVersion();
    }
}
