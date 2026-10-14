namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class WeightedRetroPayNoticeRulesChecks
{
    public static void WeightedRetroPayNoticeValidatorReportsEveryProblem()
    {
        var validator = WeightedRetroPayNoticeRules.DefaultWeightedRetroPayNoticeValidator();
        var item = WeightedRetroPayNotice.NewWeightedRetroPayNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRetroPayNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedRetroPayNoticeLabelLengthRule(120);
        var item = WeightedRetroPayNotice.NewWeightedRetroPayNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRetroPayNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedRetroPayNoticeRequiredCodeRule("weighted retro pay notice needs an entry");
        var item = WeightedRetroPayNotice.NewWeightedRetroPayNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRetroPayNoticeRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new WeightedRetroPayNoticeRequiredUnitRule("weighted retro pay notice needs an entry");
        var item = WeightedRetroPayNotice.NewWeightedRetroPayNotice("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedRetroPayNoticeValidatorReportsEveryProblem();
        WeightedRetroPayNoticeLabelLengthRuleFlagsALongLabel();
        WeightedRetroPayNoticeRequiredCodeRuleFlagsABlankCode();
        WeightedRetroPayNoticeRequiredUnitRuleFlagsABlankUnit();
    }
}
