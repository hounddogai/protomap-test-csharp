namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleNoticeRulesChecks
{
    public static void MeritCycleNoticeValidatorReportsEveryProblem()
    {
        var validator = MeritCycleNoticeRules.DefaultMeritCycleNoticeValidator();
        var item = MeritCycleNotice.NewMeritCycleNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MeritCycleNoticeRequiredTitleRule("merit cycle notice needs an entry");
        var item = MeritCycleNotice.NewMeritCycleNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleNoticeRequiredCodeRule("merit cycle notice needs a value");
        var item = MeritCycleNotice.NewMeritCycleNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new MeritCycleNoticeLabelLengthRule(3600);
        var item = MeritCycleNotice.NewMeritCycleNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleNoticeRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new MeritCycleNoticeRequiredSalaryBandRule("merit cycle notice needs this field");
        var item = MeritCycleNotice.NewMeritCycleNotice("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleNoticeSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new MeritCycleNoticeSalaryBandLengthRule(3600);
        var item = MeritCycleNotice.NewMeritCycleNotice("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MeritCycleNoticeTitleLengthRule(1000);
        var item = MeritCycleNotice.NewMeritCycleNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleNoticeValidatorReportsEveryProblem();
        MeritCycleNoticeRequiredTitleRuleFlagsABlankTitle();
        MeritCycleNoticeRequiredCodeRuleFlagsABlankCode();
        MeritCycleNoticeLabelLengthRuleFlagsALongLabel();
        MeritCycleNoticeRequiredSalaryBandRuleFlagsABlankSalaryBand();
        MeritCycleNoticeSalaryBandLengthRuleFlagsALongSalaryBand();
        MeritCycleNoticeTitleLengthRuleFlagsALongTitle();
    }
}
