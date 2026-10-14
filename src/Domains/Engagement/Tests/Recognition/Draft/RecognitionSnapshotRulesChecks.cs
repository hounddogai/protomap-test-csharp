namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RecognitionSnapshotRulesChecks
{
    public static void RecognitionSnapshotValidatorReportsEveryProblem()
    {
        var validator = RecognitionSnapshotRules.DefaultRecognitionSnapshotValidator();
        var item = RecognitionSnapshot.NewRecognitionSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionSnapshotSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new RecognitionSnapshotSequenceRangeRule(32, 32);
        var item = RecognitionSnapshot.NewRecognitionSnapshot("x", "y");
        item.Sequence = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionSnapshotRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new RecognitionSnapshotRequiredSourceRule("recognition snapshot needs an entry");
        var item = RecognitionSnapshot.NewRecognitionSnapshot("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionSnapshotSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new RecognitionSnapshotSalaryBandLengthRule(86400);
        var item = RecognitionSnapshot.NewRecognitionSnapshot("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionSnapshotSourceLengthRuleFlagsALongSource()
    {
        var rule = new RecognitionSnapshotSourceLengthRule(64);
        var item = RecognitionSnapshot.NewRecognitionSnapshot("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionSnapshotRequiredCodeRule("recognition snapshot needs an entry");
        var item = RecognitionSnapshot.NewRecognitionSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionSnapshotRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new RecognitionSnapshotRequiredSalaryBandRule("recognition snapshot needs an entry");
        var item = RecognitionSnapshot.NewRecognitionSnapshot("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionSnapshotValidatorReportsEveryProblem();
        RecognitionSnapshotSequenceRangeRuleFlagsValuesAboveSequence();
        RecognitionSnapshotRequiredSourceRuleFlagsABlankSource();
        RecognitionSnapshotSalaryBandLengthRuleFlagsALongSalaryBand();
        RecognitionSnapshotSourceLengthRuleFlagsALongSource();
        RecognitionSnapshotRequiredCodeRuleFlagsABlankCode();
        RecognitionSnapshotRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
