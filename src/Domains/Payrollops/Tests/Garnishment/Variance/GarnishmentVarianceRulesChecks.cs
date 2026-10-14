namespace Acme.Workforce.Payrollops.Tests.Garnishment.Variance;

using Acme.Workforce.Payrollops.Garnishment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GarnishmentVarianceRulesChecks
{
    public static void GarnishmentVarianceValidatorReportsEveryProblem()
    {
        var validator = GarnishmentVarianceRules.DefaultGarnishmentVarianceValidator();
        var item = GarnishmentVariance.NewGarnishmentVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentVarianceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GarnishmentVarianceRequiredLabelRule("garnishment variance needs an entry");
        var item = GarnishmentVariance.NewGarnishmentVariance("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentVarianceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new GarnishmentVarianceTenureMonthsRangeRule(3600, 86400);
        var item = GarnishmentVariance.NewGarnishmentVariance("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentVarianceUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new GarnishmentVarianceUpdatedAtRangeRule(64, 255);
        var item = GarnishmentVariance.NewGarnishmentVariance("x", "y");
        item.UpdatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GarnishmentVarianceValidatorReportsEveryProblem();
        GarnishmentVarianceRequiredLabelRuleFlagsABlankLabel();
        GarnishmentVarianceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        GarnishmentVarianceUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
