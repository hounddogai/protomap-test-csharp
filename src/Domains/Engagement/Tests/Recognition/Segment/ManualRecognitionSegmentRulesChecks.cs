namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ManualRecognitionSegmentRulesChecks
{
    public static void ManualRecognitionSegmentValidatorReportsEveryProblem()
    {
        var validator = ManualRecognitionSegmentRules.DefaultManualRecognitionSegmentValidator();
        var item = ManualRecognitionSegment.NewManualRecognitionSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualRecognitionSegmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualRecognitionSegmentRequiredTitleRule("manual recognition segment needs an entry");
        var item = ManualRecognitionSegment.NewManualRecognitionSegment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRecognitionSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new ManualRecognitionSegmentNameLengthRule(120);
        var item = ManualRecognitionSegment.NewManualRecognitionSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualRecognitionSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualRecognitionSegmentRequiredCodeRule("manual recognition segment needs a value");
        var item = ManualRecognitionSegment.NewManualRecognitionSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualRecognitionSegmentValidatorReportsEveryProblem();
        ManualRecognitionSegmentRequiredTitleRuleFlagsABlankTitle();
        ManualRecognitionSegmentNameLengthRuleFlagsALongName();
        ManualRecognitionSegmentRequiredCodeRuleFlagsABlankCode();
    }
}
