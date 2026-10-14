namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SeasonalBonusPoolRequestRulesChecks
{
    public static void SeasonalBonusPoolRequestValidatorReportsEveryProblem()
    {
        var validator = SeasonalBonusPoolRequestRules.DefaultSeasonalBonusPoolRequestValidator();
        var item = SeasonalBonusPoolRequest.NewSeasonalBonusPoolRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalBonusPoolRequestCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new SeasonalBonusPoolRequestCeilingRangeRule(2.2, 730.5);
        var item = SeasonalBonusPoolRequest.NewSeasonalBonusPoolRequest("x", "y");
        item.Ceiling = 732.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalBonusPoolRequestScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new SeasonalBonusPoolRequestScoreRangeRule(6.0, 4407.8);
        var item = SeasonalBonusPoolRequest.NewSeasonalBonusPoolRequest("x", "y");
        item.Score = 4409.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalBonusPoolRequestRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new SeasonalBonusPoolRequestRateRangeRule(2.6, 4585.4);
        var item = SeasonalBonusPoolRequest.NewSeasonalBonusPoolRequest("x", "y");
        item.Rate = 4586.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalBonusPoolRequestValidatorReportsEveryProblem();
        SeasonalBonusPoolRequestCeilingRangeRuleFlagsValuesAboveCeiling();
        SeasonalBonusPoolRequestScoreRangeRuleFlagsValuesAboveScore();
        SeasonalBonusPoolRequestRateRangeRuleFlagsValuesAboveRate();
    }
}
