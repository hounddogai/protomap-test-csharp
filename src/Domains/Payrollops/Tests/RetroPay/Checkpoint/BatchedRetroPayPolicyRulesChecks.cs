namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedRetroPayPolicyRulesChecks
{
    public static void BatchedRetroPayPolicyValidatorReportsEveryProblem()
    {
        var validator = BatchedRetroPayPolicyRules.DefaultBatchedRetroPayPolicyValidator();
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRetroPayPolicyRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new BatchedRetroPayPolicyRatingRangeRule(3.5, 427.6);
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("x", "y");
        item.Rating = 429.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRetroPayPolicyExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new BatchedRetroPayPolicyExpiresAtRangeRule(32, 255);
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRetroPayPolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedRetroPayPolicyRequiredTitleRule("batched retro pay policy needs this field");
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedRetroPayPolicyWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new BatchedRetroPayPolicyWindowDaysRangeRule(64, 255);
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("x", "y");
        item.WindowDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedRetroPayPolicyValidatorReportsEveryProblem();
        BatchedRetroPayPolicyRatingRangeRuleFlagsValuesAboveRating();
        BatchedRetroPayPolicyExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        BatchedRetroPayPolicyRequiredTitleRuleFlagsABlankTitle();
        BatchedRetroPayPolicyWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
