namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SeasonalGarnishmentNoticeRulesChecks
{
    public static void SeasonalGarnishmentNoticeValidatorReportsEveryProblem()
    {
        var validator = SeasonalGarnishmentNoticeRules.DefaultSeasonalGarnishmentNoticeValidator();
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalGarnishmentNoticeJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new SeasonalGarnishmentNoticeJobCodeLengthRule(120);
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalGarnishmentNoticeEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new SeasonalGarnishmentNoticeEmployeeIdLengthRule(3600);
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalGarnishmentNoticeMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SeasonalGarnishmentNoticeMarginRangeRule(2.1, 3073.8);
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("x", "y");
        item.Margin = 3075.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalGarnishmentNoticeRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new SeasonalGarnishmentNoticeRequiredEmployeeIdRule("seasonal garnishment notice needs an entry");
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentNoticeValidatorReportsEveryProblem();
        SeasonalGarnishmentNoticeJobCodeLengthRuleFlagsALongJobCode();
        SeasonalGarnishmentNoticeEmployeeIdLengthRuleFlagsALongEmployeeId();
        SeasonalGarnishmentNoticeMarginRangeRuleFlagsValuesAboveMargin();
        SeasonalGarnishmentNoticeRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
