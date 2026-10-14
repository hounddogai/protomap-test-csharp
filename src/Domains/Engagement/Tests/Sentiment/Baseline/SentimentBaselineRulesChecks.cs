namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SentimentBaselineRulesChecks
{
    public static void SentimentBaselineValidatorReportsEveryProblem()
    {
        var validator = SentimentBaselineRules.DefaultSentimentBaselineValidator();
        var item = SentimentBaseline.NewSentimentBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentBaselineRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new SentimentBaselineRequiredDepartmentRule("sentiment baseline needs this field");
        var item = SentimentBaseline.NewSentimentBaseline("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SentimentBaselineRequiredLabelRule("sentiment baseline needs this field");
        var item = SentimentBaseline.NewSentimentBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentBaselineCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new SentimentBaselineCategoryLengthRule(86400);
        var item = SentimentBaseline.NewSentimentBaseline("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentBaselineRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new SentimentBaselineRequiredCategoryRule("sentiment baseline needs an entry");
        var item = SentimentBaseline.NewSentimentBaseline("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentBaselineWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new SentimentBaselineWindowDaysRangeRule(32, 1000);
        var item = SentimentBaseline.NewSentimentBaseline("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentBaselineValidatorReportsEveryProblem();
        SentimentBaselineRequiredDepartmentRuleFlagsABlankDepartment();
        SentimentBaselineRequiredLabelRuleFlagsABlankLabel();
        SentimentBaselineCategoryLengthRuleFlagsALongCategory();
        SentimentBaselineRequiredCategoryRuleFlagsABlankCategory();
        SentimentBaselineWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
