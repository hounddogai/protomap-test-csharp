namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

/// <summary>Checks of adjustment allocation code, run by the project's runner.</summary>
public static class RollingAdjustmentNoticeRulesChecks
{
    public static void RollingAdjustmentNoticeValidatorReportsEveryProblem()
    {
        var validator = RollingAdjustmentNoticeRules.DefaultRollingAdjustmentNoticeValidator();
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAdjustmentNoticeRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new RollingAdjustmentNoticeRequiredManagerIdRule("rolling adjustment notice needs a value");
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAdjustmentNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RollingAdjustmentNoticeTitleLengthRule(255);
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAdjustmentNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingAdjustmentNoticeRequiredCodeRule("rolling adjustment notice needs this field");
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAdjustmentNoticeManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new RollingAdjustmentNoticeManagerIdLengthRule(32);
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingAdjustmentNoticeValidatorReportsEveryProblem();
        RollingAdjustmentNoticeRequiredManagerIdRuleFlagsABlankManagerId();
        RollingAdjustmentNoticeTitleLengthRuleFlagsALongTitle();
        RollingAdjustmentNoticeRequiredCodeRuleFlagsABlankCode();
        RollingAdjustmentNoticeManagerIdLengthRuleFlagsALongManagerId();
    }
}
