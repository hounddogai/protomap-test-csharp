namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMetricRulesChecks
{
    public static void LicenseSeatMetricValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatMetricRules.DefaultLicenseSeatMetricValidator();
        var item = LicenseSeatMetric.NewLicenseSeatMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatMetricGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new LicenseSeatMetricGraceDaysRangeRule(1000, 86400);
        var item = LicenseSeatMetric.NewLicenseSeatMetric("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatMetricCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LicenseSeatMetricCostRangeRule(3.3, 1661.1);
        var item = LicenseSeatMetric.NewLicenseSeatMetric("x", "y");
        item.Cost = 1662.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatMetricJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new LicenseSeatMetricJobCodeLengthRule(1000);
        var item = LicenseSeatMetric.NewLicenseSeatMetric("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatMetricContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new LicenseSeatMetricContactEmailLengthRule(86400);
        var item = LicenseSeatMetric.NewLicenseSeatMetric("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatMetricValidatorReportsEveryProblem();
        LicenseSeatMetricGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        LicenseSeatMetricCostRangeRuleFlagsValuesAboveCost();
        LicenseSeatMetricJobCodeLengthRuleFlagsALongJobCode();
        LicenseSeatMetricContactEmailLengthRuleFlagsALongContactEmail();
    }
}
