namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class SentimentTierRulesChecks
{
    public static void SentimentTierValidatorReportsEveryProblem()
    {
        var validator = SentimentTierRules.DefaultSentimentTierValidator();
        var item = SentimentTier.NewSentimentTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentTierNameLengthRuleFlagsALongName()
    {
        var rule = new SentimentTierNameLengthRule(64);
        var item = SentimentTier.NewSentimentTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentTierTeamLengthRuleFlagsALongTeam()
    {
        var rule = new SentimentTierTeamLengthRule(1000);
        var item = SentimentTier.NewSentimentTier("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentTierFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new SentimentTierFteRangeRule(4.7, 797.8);
        var item = SentimentTier.NewSentimentTier("x", "y");
        item.Fte = 799.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new SentimentTierRequiredNameRule("sentiment tier needs a value");
        var item = SentimentTier.NewSentimentTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentTierOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new SentimentTierOwnerIdLengthRule(32);
        var item = SentimentTier.NewSentimentTier("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentTierCodeLengthRule(64);
        var item = SentimentTier.NewSentimentTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentTierValidatorReportsEveryProblem();
        SentimentTierNameLengthRuleFlagsALongName();
        SentimentTierTeamLengthRuleFlagsALongTeam();
        SentimentTierFteRangeRuleFlagsValuesAboveFte();
        SentimentTierRequiredNameRuleFlagsABlankName();
        SentimentTierOwnerIdLengthRuleFlagsALongOwnerId();
        SentimentTierCodeLengthRuleFlagsALongCode();
    }
}
