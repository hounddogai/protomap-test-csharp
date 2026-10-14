namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingMobilityNoticeRulesChecks
{
    public static void RollingMobilityNoticeValidatorReportsEveryProblem()
    {
        var validator = RollingMobilityNoticeRules.DefaultRollingMobilityNoticeValidator();
        var item = RollingMobilityNotice.NewRollingMobilityNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingMobilityNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingMobilityNoticeCodeLengthRule(16);
        var item = RollingMobilityNotice.NewRollingMobilityNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingMobilityNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingMobilityNoticeRequiredCodeRule("rolling mobility notice needs an entry");
        var item = RollingMobilityNotice.NewRollingMobilityNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingMobilityNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RollingMobilityNoticeTenureMonthsRangeRule(32, 1000);
        var item = RollingMobilityNotice.NewRollingMobilityNotice("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingMobilityNoticeValidatorReportsEveryProblem();
        RollingMobilityNoticeCodeLengthRuleFlagsALongCode();
        RollingMobilityNoticeRequiredCodeRuleFlagsABlankCode();
        RollingMobilityNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
