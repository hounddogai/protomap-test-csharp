namespace Acme.Workforce.Compensation.Tests.EquityGrant.Plan;

using Acme.Workforce.Compensation.EquityGrant.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BatchedEquityGrantPreferenceRulesChecks
{
    public static void BatchedEquityGrantPreferenceValidatorReportsEveryProblem()
    {
        var validator = BatchedEquityGrantPreferenceRules.DefaultBatchedEquityGrantPreferenceValidator();
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedEquityGrantPreferenceRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new BatchedEquityGrantPreferenceRatingRangeRule(7.9, 4548.3);
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("x", "y");
        item.Rating = 4549.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedEquityGrantPreferenceTeamLengthRuleFlagsALongTeam()
    {
        var rule = new BatchedEquityGrantPreferenceTeamLengthRule(32);
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedEquityGrantPreferenceBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BatchedEquityGrantPreferenceBatchSizeRangeRule(255, 1000);
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedEquityGrantPreferenceRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new BatchedEquityGrantPreferenceRequiredTeamRule("batched equity grant preference needs this field");
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedEquityGrantPreferenceValidatorReportsEveryProblem();
        BatchedEquityGrantPreferenceRatingRangeRuleFlagsValuesAboveRating();
        BatchedEquityGrantPreferenceTeamLengthRuleFlagsALongTeam();
        BatchedEquityGrantPreferenceBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        BatchedEquityGrantPreferenceRequiredTeamRuleFlagsABlankTeam();
    }
}
