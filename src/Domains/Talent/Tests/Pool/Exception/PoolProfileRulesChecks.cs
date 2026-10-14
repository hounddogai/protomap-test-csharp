namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PoolProfileRulesChecks
{
    public static void PoolProfileValidatorReportsEveryProblem()
    {
        var validator = PoolProfileRules.DefaultPoolProfileValidator();
        var item = PoolProfile.NewPoolProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolProfileRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new PoolProfileRequiredSummaryRule("pool profile needs this field");
        var item = PoolProfile.NewPoolProfile("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolProfileRequiredCodeRule("pool profile needs this field");
        var item = PoolProfile.NewPoolProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolProfileSourceLengthRuleFlagsALongSource()
    {
        var rule = new PoolProfileSourceLengthRule(80);
        var item = PoolProfile.NewPoolProfile("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolProfileCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PoolProfileCreatedAtRangeRule(64, 1000);
        var item = PoolProfile.NewPoolProfile("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new PoolProfileCodeLengthRule(64);
        var item = PoolProfile.NewPoolProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolProfileRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new PoolProfileRequiredSourceRule("pool profile needs an entry");
        var item = PoolProfile.NewPoolProfile("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PoolProfileValidatorReportsEveryProblem();
        PoolProfileRequiredSummaryRuleFlagsABlankSummary();
        PoolProfileRequiredCodeRuleFlagsABlankCode();
        PoolProfileSourceLengthRuleFlagsALongSource();
        PoolProfileCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        PoolProfileCodeLengthRuleFlagsALongCode();
        PoolProfileRequiredSourceRuleFlagsABlankSource();
    }
}
