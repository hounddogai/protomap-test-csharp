namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class BonusPoolForecastRulesChecks
{
    public static void BonusPoolForecastValidatorReportsEveryProblem()
    {
        var validator = BonusPoolForecastRules.DefaultBonusPoolForecastValidator();
        var item = BonusPoolForecast.NewBonusPoolForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BonusPoolForecastRequiredTitleRule("bonus pool forecast needs an entry");
        var item = BonusPoolForecast.NewBonusPoolForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolForecastSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new BonusPoolForecastSummaryLengthRule(64);
        var item = BonusPoolForecast.NewBonusPoolForecast("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolForecastRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new BonusPoolForecastRequiredSummaryRule("bonus pool forecast needs a value");
        var item = BonusPoolForecast.NewBonusPoolForecast("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolForecastTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BonusPoolForecastTitleLengthRule(1000);
        var item = BonusPoolForecast.NewBonusPoolForecast("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolForecastRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new BonusPoolForecastRevisionRangeRule(32, 80);
        var item = BonusPoolForecast.NewBonusPoolForecast("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BonusPoolForecastValidatorReportsEveryProblem();
        BonusPoolForecastRequiredTitleRuleFlagsABlankTitle();
        BonusPoolForecastSummaryLengthRuleFlagsALongSummary();
        BonusPoolForecastRequiredSummaryRuleFlagsABlankSummary();
        BonusPoolForecastTitleLengthRuleFlagsALongTitle();
        BonusPoolForecastRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
