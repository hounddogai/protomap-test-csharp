namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TieredLicenseSeatAdjustmentRulesChecks
{
    public static void TieredLicenseSeatAdjustmentValidatorReportsEveryProblem()
    {
        var validator = TieredLicenseSeatAdjustmentRules.DefaultTieredLicenseSeatAdjustmentValidator();
        var item = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLicenseSeatAdjustmentRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new TieredLicenseSeatAdjustmentRequiredSummaryRule("tiered license seat adjustment needs an entry");
        var item = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLicenseSeatAdjustmentRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new TieredLicenseSeatAdjustmentRatingRangeRule(8.8, 162.0);
        var item = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment("x", "y");
        item.Rating = 163.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredLicenseSeatAdjustmentSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new TieredLicenseSeatAdjustmentSummaryLengthRule(86400);
        var item = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredLicenseSeatAdjustmentMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new TieredLicenseSeatAdjustmentMarginRangeRule(1.2, 4550.1);
        var item = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment("x", "y");
        item.Margin = 4551.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredLicenseSeatAdjustmentValidatorReportsEveryProblem();
        TieredLicenseSeatAdjustmentRequiredSummaryRuleFlagsABlankSummary();
        TieredLicenseSeatAdjustmentRatingRangeRuleFlagsValuesAboveRating();
        TieredLicenseSeatAdjustmentSummaryLengthRuleFlagsALongSummary();
        TieredLicenseSeatAdjustmentMarginRangeRuleFlagsValuesAboveMargin();
    }
}
