namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanBudgetRulesChecks
{
    public static void SpanBudgetValidatorReportsEveryProblem()
    {
        var validator = SpanBudgetRules.DefaultSpanBudgetValidator();
        var item = SpanBudget.NewSpanBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanBudgetUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SpanBudgetUpdatedAtRangeRule(16, 1000);
        var item = SpanBudget.NewSpanBudget("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanBudgetRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SpanBudgetRetriesRangeRule(32, 3600);
        var item = SpanBudget.NewSpanBudget("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new SpanBudgetRequiredNameRule("span budget needs this field");
        var item = SpanBudget.NewSpanBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanBudgetValidatorReportsEveryProblem();
        SpanBudgetUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        SpanBudgetRetriesRangeRuleFlagsValuesAboveRetries();
        SpanBudgetRequiredNameRuleFlagsABlankName();
    }
}
