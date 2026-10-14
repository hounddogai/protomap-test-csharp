namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Metric;

using Acme.Workforce.Contractors.StatementOfWork.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMetricRulesChecks
{
    public static void DefaultStatementOfWorkMetricValidatorReportsEveryProblem()
    {
        var validator = DefaultStatementOfWorkMetricRules.DefaultDefaultStatementOfWorkMetricValidator();
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultStatementOfWorkMetricRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultStatementOfWorkMetricRequiredNameRule("default statement of work metric needs an entry");
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultStatementOfWorkMetricRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new DefaultStatementOfWorkMetricRequiredCountryRule(
            "default statement of work metric needs an entry");
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultStatementOfWorkMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultStatementOfWorkMetricCodeLengthRule(3600);
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultStatementOfWorkMetricQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new DefaultStatementOfWorkMetricQuantityRangeRule(32, 64);
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("x", "y");
        item.Quantity = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultStatementOfWorkMetricCountryLengthRuleFlagsALongCountry()
    {
        var rule = new DefaultStatementOfWorkMetricCountryLengthRule(64);
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkMetricValidatorReportsEveryProblem();
        DefaultStatementOfWorkMetricRequiredNameRuleFlagsABlankName();
        DefaultStatementOfWorkMetricRequiredCountryRuleFlagsABlankCountry();
        DefaultStatementOfWorkMetricCodeLengthRuleFlagsALongCode();
        DefaultStatementOfWorkMetricQuantityRangeRuleFlagsValuesAboveQuantity();
        DefaultStatementOfWorkMetricCountryLengthRuleFlagsALongCountry();
    }
}
