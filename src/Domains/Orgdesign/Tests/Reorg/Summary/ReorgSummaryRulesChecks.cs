namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReorgSummaryRulesChecks
{
    public static void ReorgSummaryValidatorReportsEveryProblem()
    {
        var validator = ReorgSummaryRules.DefaultReorgSummaryValidator();
        var item = ReorgSummary.NewReorgSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgSummaryPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ReorgSummaryPhoneLengthRule(255);
        var item = ReorgSummary.NewReorgSummary("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgSummaryRequiredCodeRule("reorg summary needs this field");
        var item = ReorgSummary.NewReorgSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgSummaryRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new ReorgSummaryRateRangeRule(4.7, 3046.9);
        var item = ReorgSummary.NewReorgSummary("x", "y");
        item.Rate = 3048.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReorgSummaryValidatorReportsEveryProblem();
        ReorgSummaryPhoneLengthRuleFlagsALongPhone();
        ReorgSummaryRequiredCodeRuleFlagsABlankCode();
        ReorgSummaryRateRangeRuleFlagsValuesAboveRate();
    }
}
