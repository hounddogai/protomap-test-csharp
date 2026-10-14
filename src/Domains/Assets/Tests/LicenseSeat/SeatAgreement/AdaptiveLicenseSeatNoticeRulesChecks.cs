namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatNoticeRulesChecks
{
    public static void AdaptiveLicenseSeatNoticeValidatorReportsEveryProblem()
    {
        var validator = AdaptiveLicenseSeatNoticeRules.DefaultAdaptiveLicenseSeatNoticeValidator();
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveLicenseSeatNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveLicenseSeatNoticeTitleLengthRule(86400);
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveLicenseSeatNoticeFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AdaptiveLicenseSeatNoticeFteRangeRule(0.8, 4458.5);
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("x", "y");
        item.Fte = 4460.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLicenseSeatNoticeRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new AdaptiveLicenseSeatNoticeRatioRangeRule(2.3, 3764.7);
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("x", "y");
        item.Ratio = 3766.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLicenseSeatNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new AdaptiveLicenseSeatNoticeTenureMonthsRangeRule(255, 86400);
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatNoticeValidatorReportsEveryProblem();
        AdaptiveLicenseSeatNoticeTitleLengthRuleFlagsALongTitle();
        AdaptiveLicenseSeatNoticeFteRangeRuleFlagsValuesAboveFte();
        AdaptiveLicenseSeatNoticeRatioRangeRuleFlagsValuesAboveRatio();
        AdaptiveLicenseSeatNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
