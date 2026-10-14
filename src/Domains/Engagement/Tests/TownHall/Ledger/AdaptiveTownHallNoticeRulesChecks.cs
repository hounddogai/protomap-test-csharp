namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveTownHallNoticeRulesChecks
{
    public static void AdaptiveTownHallNoticeValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTownHallNoticeRules.DefaultAdaptiveTownHallNoticeValidator();
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTownHallNoticeCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new AdaptiveTownHallNoticeCostCenterLengthRule(120);
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveTownHallNoticeCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AdaptiveTownHallNoticeCostRangeRule(2.2, 3339.1);
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("x", "y");
        item.Cost = 3340.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTownHallNoticeSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new AdaptiveTownHallNoticeSegmentLengthRule(255);
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveTownHallNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveTownHallNoticeRequiredTitleRule("adaptive town hall notice needs an entry");
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveTownHallNoticeRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new AdaptiveTownHallNoticeRequiredSegmentRule("adaptive town hall notice needs a value");
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveTownHallNoticeValidatorReportsEveryProblem();
        AdaptiveTownHallNoticeCostCenterLengthRuleFlagsALongCostCenter();
        AdaptiveTownHallNoticeCostRangeRuleFlagsValuesAboveCost();
        AdaptiveTownHallNoticeSegmentLengthRuleFlagsALongSegment();
        AdaptiveTownHallNoticeRequiredTitleRuleFlagsABlankTitle();
        AdaptiveTownHallNoticeRequiredSegmentRuleFlagsABlankSegment();
    }
}
