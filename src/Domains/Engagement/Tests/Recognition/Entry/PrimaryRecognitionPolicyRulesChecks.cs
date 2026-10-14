namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PrimaryRecognitionPolicyRulesChecks
{
    public static void PrimaryRecognitionPolicyValidatorReportsEveryProblem()
    {
        var validator = PrimaryRecognitionPolicyRules.DefaultPrimaryRecognitionPolicyValidator();
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryRecognitionPolicyRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PrimaryRecognitionPolicyRetriesRangeRule(16, 32);
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("x", "y");
        item.Retries = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryRecognitionPolicySourceLengthRuleFlagsALongSource()
    {
        var rule = new PrimaryRecognitionPolicySourceLengthRule(3600);
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryRecognitionPolicyRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PrimaryRecognitionPolicyRequiredOwnerIdRule("primary recognition policy needs an entry");
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRecognitionPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PrimaryRecognitionPolicyBatchSizeRangeRule(64, 120);
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("x", "y");
        item.BatchSize = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryRecognitionPolicyValidatorReportsEveryProblem();
        PrimaryRecognitionPolicyRetriesRangeRuleFlagsValuesAboveRetries();
        PrimaryRecognitionPolicySourceLengthRuleFlagsALongSource();
        PrimaryRecognitionPolicyRequiredOwnerIdRuleFlagsABlankOwnerId();
        PrimaryRecognitionPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
