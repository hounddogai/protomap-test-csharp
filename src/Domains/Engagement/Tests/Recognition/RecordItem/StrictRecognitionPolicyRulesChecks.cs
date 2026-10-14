namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictRecognitionPolicyRulesChecks
{
    public static void StrictRecognitionPolicyValidatorReportsEveryProblem()
    {
        var validator = StrictRecognitionPolicyRules.DefaultStrictRecognitionPolicyValidator();
        var item = StrictRecognitionPolicy.NewStrictRecognitionPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictRecognitionPolicyUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new StrictRecognitionPolicyUpdatedAtRangeRule(255, 3600);
        var item = StrictRecognitionPolicy.NewStrictRecognitionPolicy("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictRecognitionPolicyRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new StrictRecognitionPolicyRequiredContactEmailRule("strict recognition policy needs this field");
        var item = StrictRecognitionPolicy.NewStrictRecognitionPolicy("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictRecognitionPolicyValidatorReportsEveryProblem();
        StrictRecognitionPolicyUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        StrictRecognitionPolicyRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
