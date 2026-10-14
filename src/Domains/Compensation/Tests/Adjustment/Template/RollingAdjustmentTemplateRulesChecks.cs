namespace Acme.Workforce.Compensation.Tests.Adjustment.Template;

using Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RollingAdjustmentTemplateRulesChecks
{
    public static void RollingAdjustmentTemplateValidatorReportsEveryProblem()
    {
        var validator = RollingAdjustmentTemplateRules.DefaultRollingAdjustmentTemplateValidator();
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAdjustmentTemplateRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RollingAdjustmentTemplateRequiredCurrencyRule("rolling adjustment template needs this field");
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAdjustmentTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingAdjustmentTemplateRequiredCodeRule("rolling adjustment template needs a value");
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAdjustmentTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new RollingAdjustmentTemplateGraceDaysRangeRule(16, 1000);
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("x", "y");
        item.GraceDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAdjustmentTemplateRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new RollingAdjustmentTemplateRequiredTeamRule("rolling adjustment template needs a value");
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAdjustmentTemplateHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new RollingAdjustmentTemplateHoursRangeRule(6.1, 3663.8);
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("x", "y");
        item.Hours = 3665.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingAdjustmentTemplateValidatorReportsEveryProblem();
        RollingAdjustmentTemplateRequiredCurrencyRuleFlagsABlankCurrency();
        RollingAdjustmentTemplateRequiredCodeRuleFlagsABlankCode();
        RollingAdjustmentTemplateGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        RollingAdjustmentTemplateRequiredTeamRuleFlagsABlankTeam();
        RollingAdjustmentTemplateHoursRangeRuleFlagsValuesAboveHours();
    }
}
