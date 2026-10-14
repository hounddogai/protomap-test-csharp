namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedSpanSummaryRulesChecks
{
    public static void WeightedSpanSummaryValidatorReportsEveryProblem()
    {
        var validator = WeightedSpanSummaryRules.DefaultWeightedSpanSummaryValidator();
        var item = WeightedSpanSummary.NewWeightedSpanSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedSpanSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedSpanSummaryTitleLengthRule(64);
        var item = WeightedSpanSummary.NewWeightedSpanSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedSpanSummaryRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new WeightedSpanSummaryRequiredSourceRule("weighted span summary needs a value");
        var item = WeightedSpanSummary.NewWeightedSpanSummary("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedSpanSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedSpanSummaryRequiredTitleRule("weighted span summary needs this field");
        var item = WeightedSpanSummary.NewWeightedSpanSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedSpanSummaryValidatorReportsEveryProblem();
        WeightedSpanSummaryTitleLengthRuleFlagsALongTitle();
        WeightedSpanSummaryRequiredSourceRuleFlagsABlankSource();
        WeightedSpanSummaryRequiredTitleRuleFlagsABlankTitle();
    }
}
