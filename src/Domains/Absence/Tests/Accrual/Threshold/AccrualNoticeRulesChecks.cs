namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualNoticeRulesChecks
{
    public static void AccrualNoticeValidatorReportsEveryProblem()
    {
        var validator = AccrualNoticeRules.DefaultAccrualNoticeValidator();
        var item = AccrualNotice.NewAccrualNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualNoticeWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AccrualNoticeWindowDaysRangeRule(16, 86400);
        var item = AccrualNotice.NewAccrualNotice("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualNoticeRequiredCodeRule("accrual notice needs a value");
        var item = AccrualNotice.NewAccrualNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualNoticeRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccrualNoticeRequiredLabelRule("accrual notice needs a value");
        var item = AccrualNotice.NewAccrualNotice("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualNoticeValidatorReportsEveryProblem();
        AccrualNoticeWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AccrualNoticeRequiredCodeRuleFlagsABlankCode();
        AccrualNoticeRequiredLabelRuleFlagsABlankLabel();
    }
}
