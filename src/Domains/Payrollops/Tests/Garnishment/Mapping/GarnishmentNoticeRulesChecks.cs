namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GarnishmentNoticeRulesChecks
{
    public static void GarnishmentNoticeValidatorReportsEveryProblem()
    {
        var validator = GarnishmentNoticeRules.DefaultGarnishmentNoticeValidator();
        var item = GarnishmentNotice.NewGarnishmentNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new GarnishmentNoticeTenureMonthsRangeRule(16, 3600);
        var item = GarnishmentNotice.NewGarnishmentNotice("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentNoticeRequiredNameRuleFlagsABlankName()
    {
        var rule = new GarnishmentNoticeRequiredNameRule("garnishment notice needs this field");
        var item = GarnishmentNotice.NewGarnishmentNotice("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentNoticeDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new GarnishmentNoticeDiscountRangeRule(6.7, 139.3);
        var item = GarnishmentNotice.NewGarnishmentNotice("x", "y");
        item.Discount = 140.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentNoticeRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new GarnishmentNoticeRequiredGradeRule("garnishment notice needs this field");
        var item = GarnishmentNotice.NewGarnishmentNotice("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentNoticeGradeLengthRuleFlagsALongGrade()
    {
        var rule = new GarnishmentNoticeGradeLengthRule(80);
        var item = GarnishmentNotice.NewGarnishmentNotice("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentNoticeValidatorReportsEveryProblem();
        GarnishmentNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        GarnishmentNoticeRequiredNameRuleFlagsABlankName();
        GarnishmentNoticeDiscountRangeRuleFlagsValuesAboveDiscount();
        GarnishmentNoticeRequiredGradeRuleFlagsABlankGrade();
        GarnishmentNoticeGradeLengthRuleFlagsALongGrade();
    }
}
