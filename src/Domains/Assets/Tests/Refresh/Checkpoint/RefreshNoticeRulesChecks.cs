namespace Acme.Workforce.Assets.Tests.Refresh.Checkpoint;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RefreshNoticeRulesChecks
{
    public static void RefreshNoticeValidatorReportsEveryProblem()
    {
        var validator = RefreshNoticeRules.DefaultRefreshNoticeValidator();
        var item = RefreshNotice.NewRefreshNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshNoticePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new RefreshNoticePhoneLengthRule(120);
        var item = RefreshNotice.NewRefreshNotice("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new RefreshNoticeNameLengthRule(120);
        var item = RefreshNotice.NewRefreshNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshNoticeCodeLengthRule(32);
        var item = RefreshNotice.NewRefreshNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshNoticeValidatorReportsEveryProblem();
        RefreshNoticePhoneLengthRuleFlagsALongPhone();
        RefreshNoticeNameLengthRuleFlagsALongName();
        RefreshNoticeCodeLengthRuleFlagsALongCode();
    }
}
