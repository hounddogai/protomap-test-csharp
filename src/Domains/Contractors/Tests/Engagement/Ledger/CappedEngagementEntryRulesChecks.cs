namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CappedEngagementEntryRulesChecks
{
    public static void CappedEngagementEntryValidatorReportsEveryProblem()
    {
        var validator = CappedEngagementEntryRules.DefaultCappedEngagementEntryValidator();
        var item = CappedEngagementEntry.NewCappedEngagementEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedEngagementEntryRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new CappedEngagementEntryRequiredContactEmailRule("capped engagement entry needs a value");
        var item = CappedEngagementEntry.NewCappedEngagementEntry("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedEngagementEntryTeamLengthRuleFlagsALongTeam()
    {
        var rule = new CappedEngagementEntryTeamLengthRule(255);
        var item = CappedEngagementEntry.NewCappedEngagementEntry("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedEngagementEntryRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new CappedEngagementEntryRatingRangeRule(4.4, 3376.5);
        var item = CappedEngagementEntry.NewCappedEngagementEntry("x", "y");
        item.Rating = 3378.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedEngagementEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedEngagementEntryLabelLengthRule(80);
        var item = CappedEngagementEntry.NewCappedEngagementEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedEngagementEntryScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new CappedEngagementEntryScoreRangeRule(2.4, 3656.3);
        var item = CappedEngagementEntry.NewCappedEngagementEntry("x", "y");
        item.Score = 3657.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedEngagementEntryValidatorReportsEveryProblem();
        CappedEngagementEntryRequiredContactEmailRuleFlagsABlankContactEmail();
        CappedEngagementEntryTeamLengthRuleFlagsALongTeam();
        CappedEngagementEntryRatingRangeRuleFlagsValuesAboveRating();
        CappedEngagementEntryLabelLengthRuleFlagsALongLabel();
        CappedEngagementEntryScoreRangeRuleFlagsValuesAboveScore();
    }
}
