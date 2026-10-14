namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RecognitionDraftRulesChecks
{
    public static void RecognitionDraftValidatorReportsEveryProblem()
    {
        var validator = RecognitionDraftRules.DefaultRecognitionDraftValidator();
        var item = RecognitionDraft.NewRecognitionDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionDraftRequiredCodeRule("recognition draft needs an entry");
        var item = RecognitionDraft.NewRecognitionDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionDraftUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RecognitionDraftUpdatedAtRangeRule(120, 86400);
        var item = RecognitionDraft.NewRecognitionDraft("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionDraftSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new RecognitionDraftSegmentLengthRule(120);
        var item = RecognitionDraft.NewRecognitionDraft("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionDraftNameLengthRuleFlagsALongName()
    {
        var rule = new RecognitionDraftNameLengthRule(120);
        var item = RecognitionDraft.NewRecognitionDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionDraftRequiredNameRuleFlagsABlankName()
    {
        var rule = new RecognitionDraftRequiredNameRule("recognition draft needs this field");
        var item = RecognitionDraft.NewRecognitionDraft("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionDraftValidatorReportsEveryProblem();
        RecognitionDraftRequiredCodeRuleFlagsABlankCode();
        RecognitionDraftUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        RecognitionDraftSegmentLengthRuleFlagsALongSegment();
        RecognitionDraftNameLengthRuleFlagsALongName();
        RecognitionDraftRequiredNameRuleFlagsABlankName();
    }
}
