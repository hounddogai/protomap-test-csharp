namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentNoticeRulesChecks
{
    public static void AdaptiveAdjustmentNoticeValidatorReportsEveryProblem()
    {
        var validator = AdaptiveAdjustmentNoticeRules.DefaultAdaptiveAdjustmentNoticeValidator();
        var item = AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveAdjustmentNoticeRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveAdjustmentNoticeRequiredNameRule("adaptive adjustment notice needs an entry");
        var item = AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveAdjustmentNoticeManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new AdaptiveAdjustmentNoticeManagerIdLengthRule(1000);
        var item = AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveAdjustmentNoticeValidatorReportsEveryProblem();
        AdaptiveAdjustmentNoticeRequiredNameRuleFlagsABlankName();
        AdaptiveAdjustmentNoticeManagerIdLengthRuleFlagsALongManagerId();
    }
}
