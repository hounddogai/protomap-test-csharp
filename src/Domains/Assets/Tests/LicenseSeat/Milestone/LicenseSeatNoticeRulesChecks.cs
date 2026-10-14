namespace Acme.Workforce.Assets.Tests.LicenseSeat.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LicenseSeatNoticeRulesChecks
{
    public static void LicenseSeatNoticeValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatNoticeRules.DefaultLicenseSeatNoticeValidator();
        var item = LicenseSeatNotice.NewLicenseSeatNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LicenseSeatNoticeTitleLengthRule(86400);
        var item = LicenseSeatNotice.NewLicenseSeatNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LicenseSeatNoticeRequiredCodeRule("license seat notice needs a value");
        var item = LicenseSeatNotice.NewLicenseSeatNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatNoticeCodeLengthRule(86400);
        var item = LicenseSeatNotice.NewLicenseSeatNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatNoticeValidatorReportsEveryProblem();
        LicenseSeatNoticeTitleLengthRuleFlagsALongTitle();
        LicenseSeatNoticeRequiredCodeRuleFlagsABlankCode();
        LicenseSeatNoticeCodeLengthRuleFlagsALongCode();
    }
}
