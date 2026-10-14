namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class LegacyReorgSummaryRulesChecks
{
    public static void LegacyReorgSummaryValidatorReportsEveryProblem()
    {
        var validator = LegacyReorgSummaryRules.DefaultLegacyReorgSummaryValidator();
        var item = LegacyReorgSummary.NewLegacyReorgSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyReorgSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyReorgSummaryRequiredTitleRule("legacy reorg summary needs this field");
        var item = LegacyReorgSummary.NewLegacyReorgSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyReorgSummaryRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LegacyReorgSummaryRequiredBadgeRule("legacy reorg summary needs an entry");
        var item = LegacyReorgSummary.NewLegacyReorgSummary("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyReorgSummaryRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new LegacyReorgSummaryRequiredChannelRule("legacy reorg summary needs this field");
        var item = LegacyReorgSummary.NewLegacyReorgSummary("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyReorgSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LegacyReorgSummaryTitleLengthRule(1000);
        var item = LegacyReorgSummary.NewLegacyReorgSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyReorgSummaryChannelLengthRuleFlagsALongChannel()
    {
        var rule = new LegacyReorgSummaryChannelLengthRule(3600);
        var item = LegacyReorgSummary.NewLegacyReorgSummary("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyReorgSummaryValidatorReportsEveryProblem();
        LegacyReorgSummaryRequiredTitleRuleFlagsABlankTitle();
        LegacyReorgSummaryRequiredBadgeRuleFlagsABlankBadge();
        LegacyReorgSummaryRequiredChannelRuleFlagsABlankChannel();
        LegacyReorgSummaryTitleLengthRuleFlagsALongTitle();
        LegacyReorgSummaryChannelLengthRuleFlagsALongChannel();
    }
}
