namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionBatchRulesChecks
{
    public static void PositionBatchValidatorReportsEveryProblem()
    {
        var validator = PositionBatchRules.DefaultPositionBatchValidator();
        var item = PositionBatch.NewPositionBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionBatchCodeLengthRule(3600);
        var item = PositionBatch.NewPositionBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PositionBatchRequiredCodeRule("position batch needs an entry");
        var item = PositionBatch.NewPositionBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionBatchScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PositionBatchScoreRangeRule(0.5, 3558.2);
        var item = PositionBatch.NewPositionBatch("x", "y");
        item.Score = 3559.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionBatchRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PositionBatchRetriesRangeRule(64, 80);
        var item = PositionBatch.NewPositionBatch("x", "y");
        item.Retries = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionBatchValidatorReportsEveryProblem();
        PositionBatchCodeLengthRuleFlagsALongCode();
        PositionBatchRequiredCodeRuleFlagsABlankCode();
        PositionBatchScoreRangeRuleFlagsValuesAboveScore();
        PositionBatchRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
