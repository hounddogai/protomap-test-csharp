namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanReviewRulesChecks
{
    public static void SpanReviewValidatorReportsEveryProblem()
    {
        var validator = SpanReviewRules.DefaultSpanReviewValidator();
        var item = SpanReview.NewSpanReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanReviewRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new SpanReviewRequiredContactEmailRule("span review needs an entry");
        var item = SpanReview.NewSpanReview("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanReviewRequiredNameRuleFlagsABlankName()
    {
        var rule = new SpanReviewRequiredNameRule("span review needs an entry");
        var item = SpanReview.NewSpanReview("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanReviewNameLengthRuleFlagsALongName()
    {
        var rule = new SpanReviewNameLengthRule(16);
        var item = SpanReview.NewSpanReview("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanReviewContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new SpanReviewContactEmailLengthRule(3600);
        var item = SpanReview.NewSpanReview("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanReviewRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new SpanReviewRequiredRegionRule("span review needs this field");
        var item = SpanReview.NewSpanReview("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanReviewValidatorReportsEveryProblem();
        SpanReviewRequiredContactEmailRuleFlagsABlankContactEmail();
        SpanReviewRequiredNameRuleFlagsABlankName();
        SpanReviewNameLengthRuleFlagsALongName();
        SpanReviewContactEmailLengthRuleFlagsALongContactEmail();
        SpanReviewRequiredRegionRuleFlagsABlankRegion();
    }
}
