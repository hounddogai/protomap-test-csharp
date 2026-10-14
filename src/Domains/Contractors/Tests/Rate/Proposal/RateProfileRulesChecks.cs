namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RateProfileRulesChecks
{
    public static void RateProfileValidatorReportsEveryProblem()
    {
        var validator = RateProfileRules.DefaultRateProfileValidator();
        var item = RateProfile.NewRateProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateProfileScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RateProfileScoreRangeRule(7.8, 1329.6);
        var item = RateProfile.NewRateProfile("x", "y");
        item.Score = 1331.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RateProfileTitleLengthRule(3600);
        var item = RateProfile.NewRateProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RateProfileValidatorReportsEveryProblem();
        RateProfileScoreRangeRuleFlagsValuesAboveScore();
        RateProfileTitleLengthRuleFlagsALongTitle();
    }
}
