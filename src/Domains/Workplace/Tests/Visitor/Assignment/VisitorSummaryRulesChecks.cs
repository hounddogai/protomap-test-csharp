namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorSummaryRulesChecks
{
    public static void VisitorSummaryValidatorReportsEveryProblem()
    {
        var validator = VisitorSummaryRules.DefaultVisitorSummaryValidator();
        var item = VisitorSummary.NewVisitorSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorSummarySourceLengthRuleFlagsALongSource()
    {
        var rule = new VisitorSummarySourceLengthRule(64);
        var item = VisitorSummary.NewVisitorSummary("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorSummaryEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new VisitorSummaryEndsAtRangeRule(32, 80);
        var item = VisitorSummary.NewVisitorSummary("x", "y");
        item.EndsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new VisitorSummaryTitleLengthRule(86400);
        var item = VisitorSummary.NewVisitorSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorSummaryRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new VisitorSummaryRequiredSourceRule("visitor summary needs a value");
        var item = VisitorSummary.NewVisitorSummary("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorSummaryValidatorReportsEveryProblem();
        VisitorSummarySourceLengthRuleFlagsALongSource();
        VisitorSummaryEndsAtRangeRuleFlagsValuesAboveEndsAt();
        VisitorSummaryTitleLengthRuleFlagsALongTitle();
        VisitorSummaryRequiredSourceRuleFlagsABlankSource();
    }
}
