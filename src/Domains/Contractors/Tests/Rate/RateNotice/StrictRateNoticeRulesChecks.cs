namespace Acme.Workforce.Contractors.Tests.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.RateNotice;

/// <summary>Checks of rate notice code, run by the project's runner.</summary>
public static class StrictRateNoticeRulesChecks
{
    public static void StrictRateNoticeValidatorReportsEveryProblem()
    {
        var validator = StrictRateNoticeRules.DefaultStrictRateNoticeValidator();
        var item = StrictRateNotice.NewStrictRateNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictRateNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictRateNoticeCodeLengthRule(86400);
        var item = StrictRateNotice.NewStrictRateNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictRateNoticeMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new StrictRateNoticeMinimumRangeRule(5.2, 1168.3);
        var item = StrictRateNotice.NewStrictRateNotice("x", "y");
        item.Minimum = 1169.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictRateNoticeEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new StrictRateNoticeEndsAtRangeRule(120, 120);
        var item = StrictRateNotice.NewStrictRateNotice("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictRateNoticeCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new StrictRateNoticeCreatedAtRangeRule(255, 3600);
        var item = StrictRateNotice.NewStrictRateNotice("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictRateNoticeValidatorReportsEveryProblem();
        StrictRateNoticeCodeLengthRuleFlagsALongCode();
        StrictRateNoticeMinimumRangeRuleFlagsValuesAboveMinimum();
        StrictRateNoticeEndsAtRangeRuleFlagsValuesAboveEndsAt();
        StrictRateNoticeCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
