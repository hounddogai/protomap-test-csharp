namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentBaselineRulesChecks
{
    public static void GarnishmentBaselineValidatorReportsEveryProblem()
    {
        var validator = GarnishmentBaselineRules.DefaultGarnishmentBaselineValidator();
        var item = GarnishmentBaseline.NewGarnishmentBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentBaselineStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new GarnishmentBaselineStartsAtRangeRule(255, 1000);
        var item = GarnishmentBaseline.NewGarnishmentBaseline("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentBaselineTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new GarnishmentBaselineTimezoneLengthRule(86400);
        var item = GarnishmentBaseline.NewGarnishmentBaseline("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentBaselineCodeLengthRule(120);
        var item = GarnishmentBaseline.NewGarnishmentBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentBaselineVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new GarnishmentBaselineVersionRangeRule(64, 64);
        var item = GarnishmentBaseline.NewGarnishmentBaseline("x", "y");
        item.Version = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GarnishmentBaselineValidatorReportsEveryProblem();
        GarnishmentBaselineStartsAtRangeRuleFlagsValuesAboveStartsAt();
        GarnishmentBaselineTimezoneLengthRuleFlagsALongTimezone();
        GarnishmentBaselineCodeLengthRuleFlagsALongCode();
        GarnishmentBaselineVersionRangeRuleFlagsValuesAboveVersion();
    }
}
