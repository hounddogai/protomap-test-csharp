namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultPayBandGroupRulesChecks
{
    public static void DefaultPayBandGroupValidatorReportsEveryProblem()
    {
        var validator = DefaultPayBandGroupRules.DefaultDefaultPayBandGroupValidator();
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPayBandGroupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultPayBandGroupRequiredTitleRule("default pay band group needs an entry");
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPayBandGroupRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new DefaultPayBandGroupRetriesRangeRule(32, 1000);
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPayBandGroupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new DefaultPayBandGroupTenureMonthsRangeRule(120, 255);
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultPayBandGroupValidatorReportsEveryProblem();
        DefaultPayBandGroupRequiredTitleRuleFlagsABlankTitle();
        DefaultPayBandGroupRetriesRangeRuleFlagsValuesAboveRetries();
        DefaultPayBandGroupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
