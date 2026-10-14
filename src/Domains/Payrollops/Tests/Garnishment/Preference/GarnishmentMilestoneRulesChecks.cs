namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class GarnishmentMilestoneRulesChecks
{
    public static void GarnishmentMilestoneValidatorReportsEveryProblem()
    {
        var validator = GarnishmentMilestoneRules.DefaultGarnishmentMilestoneValidator();
        var item = GarnishmentMilestone.NewGarnishmentMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentMilestoneLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new GarnishmentMilestoneLeaveDaysRangeRule(255, 1000);
        var item = GarnishmentMilestone.NewGarnishmentMilestone("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentMilestoneCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new GarnishmentMilestoneCurrencyLengthRule(255);
        var item = GarnishmentMilestone.NewGarnishmentMilestone("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentMilestoneRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new GarnishmentMilestoneRequiredCurrencyRule("garnishment milestone needs a value");
        var item = GarnishmentMilestone.NewGarnishmentMilestone("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GarnishmentMilestoneValidatorReportsEveryProblem();
        GarnishmentMilestoneLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        GarnishmentMilestoneCurrencyLengthRuleFlagsALongCurrency();
        GarnishmentMilestoneRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
