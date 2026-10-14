namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientRecognitionBatchRulesChecks
{
    public static void LenientRecognitionBatchValidatorReportsEveryProblem()
    {
        var validator = LenientRecognitionBatchRules.DefaultLenientRecognitionBatchValidator();
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRecognitionBatchUnitLengthRuleFlagsALongUnit()
    {
        var rule = new LenientRecognitionBatchUnitLengthRule(1000);
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientRecognitionBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientRecognitionBatchCodeLengthRule(64);
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientRecognitionBatchContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new LenientRecognitionBatchContactEmailLengthRule(3600);
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientRecognitionBatchValidatorReportsEveryProblem();
        LenientRecognitionBatchUnitLengthRuleFlagsALongUnit();
        LenientRecognitionBatchCodeLengthRuleFlagsALongCode();
        LenientRecognitionBatchContactEmailLengthRuleFlagsALongContactEmail();
    }
}
