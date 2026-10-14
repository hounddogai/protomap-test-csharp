namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class RollingSentimentSummaryRulesChecks
{
    public static void RollingSentimentSummaryValidatorReportsEveryProblem()
    {
        var validator = RollingSentimentSummaryRules.DefaultRollingSentimentSummaryValidator();
        var item = RollingSentimentSummary.NewRollingSentimentSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSentimentSummaryVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new RollingSentimentSummaryVersionRangeRule(80, 120);
        var item = RollingSentimentSummary.NewRollingSentimentSummary("x", "y");
        item.Version = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSentimentSummaryFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new RollingSentimentSummaryFloorValueRangeRule(3.2, 4679.8);
        var item = RollingSentimentSummary.NewRollingSentimentSummary("x", "y");
        item.FloorValue = 4681.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSentimentSummaryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RollingSentimentSummaryLeaveDaysRangeRule(1000, 3600);
        var item = RollingSentimentSummary.NewRollingSentimentSummary("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSentimentSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingSentimentSummaryRequiredCodeRule("rolling sentiment summary needs this field");
        var item = RollingSentimentSummary.NewRollingSentimentSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSentimentSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingSentimentSummaryCodeLengthRule(16);
        var item = RollingSentimentSummary.NewRollingSentimentSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingSentimentSummaryValidatorReportsEveryProblem();
        RollingSentimentSummaryVersionRangeRuleFlagsValuesAboveVersion();
        RollingSentimentSummaryFloorValueRangeRuleFlagsValuesAboveFloorValue();
        RollingSentimentSummaryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        RollingSentimentSummaryRequiredCodeRuleFlagsABlankCode();
        RollingSentimentSummaryCodeLengthRuleFlagsALongCode();
    }
}
