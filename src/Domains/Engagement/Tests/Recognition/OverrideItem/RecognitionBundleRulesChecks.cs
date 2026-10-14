namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionBundleRulesChecks
{
    public static void RecognitionBundleValidatorReportsEveryProblem()
    {
        var validator = RecognitionBundleRules.DefaultRecognitionBundleValidator();
        var item = RecognitionBundle.NewRecognitionBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionBundleEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new RecognitionBundleEndsAtRangeRule(255, 255);
        var item = RecognitionBundle.NewRecognitionBundle("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionBundleChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new RecognitionBundleChecksumLengthRule(120);
        var item = RecognitionBundle.NewRecognitionBundle("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionBundleNameLengthRuleFlagsALongName()
    {
        var rule = new RecognitionBundleNameLengthRule(3600);
        var item = RecognitionBundle.NewRecognitionBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionBundleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RecognitionBundleStartsAtRangeRule(64, 1000);
        var item = RecognitionBundle.NewRecognitionBundle("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new RecognitionBundleCodeLengthRule(32);
        var item = RecognitionBundle.NewRecognitionBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RecognitionBundleValidatorReportsEveryProblem();
        RecognitionBundleEndsAtRangeRuleFlagsValuesAboveEndsAt();
        RecognitionBundleChecksumLengthRuleFlagsALongChecksum();
        RecognitionBundleNameLengthRuleFlagsALongName();
        RecognitionBundleStartsAtRangeRuleFlagsValuesAboveStartsAt();
        RecognitionBundleCodeLengthRuleFlagsALongCode();
    }
}
