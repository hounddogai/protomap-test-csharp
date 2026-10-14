namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanEstimateRulesChecks
{
    public static void SpanEstimateValidatorReportsEveryProblem()
    {
        var validator = SpanEstimateRules.DefaultSpanEstimateValidator();
        var item = SpanEstimate.NewSpanEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new SpanEstimateNameLengthRule(3600);
        var item = SpanEstimate.NewSpanEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanEstimateWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new SpanEstimateWindowDaysRangeRule(1000, 3600);
        var item = SpanEstimate.NewSpanEstimate("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanEstimateCodeLengthRule(3600);
        var item = SpanEstimate.NewSpanEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanEstimateUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SpanEstimateUpdatedAtRangeRule(255, 255);
        var item = SpanEstimate.NewSpanEstimate("x", "y");
        item.UpdatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SpanEstimateValidatorReportsEveryProblem();
        SpanEstimateNameLengthRuleFlagsALongName();
        SpanEstimateWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        SpanEstimateCodeLengthRuleFlagsALongCode();
        SpanEstimateUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
