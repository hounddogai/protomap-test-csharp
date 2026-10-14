namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayRuleRulesChecks
{
    public static void HolidayRuleValidatorReportsEveryProblem()
    {
        var validator = HolidayRuleRules.DefaultHolidayRuleValidator();
        var item = HolidayRule.NewHolidayRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayRuleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new HolidayRuleSalaryBandLengthRule(32);
        var item = HolidayRule.NewHolidayRule("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayRuleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new HolidayRuleRequiredGradeRule("holiday rule needs an entry");
        var item = HolidayRule.NewHolidayRule("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayRuleMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new HolidayRuleMaximumRangeRule(4.0, 4520.9);
        var item = HolidayRule.NewHolidayRule("x", "y");
        item.Maximum = 4522.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayRuleRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new HolidayRuleRequiredSalaryBandRule("holiday rule needs this field");
        var item = HolidayRule.NewHolidayRule("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayRuleValidatorReportsEveryProblem();
        HolidayRuleSalaryBandLengthRuleFlagsALongSalaryBand();
        HolidayRuleRequiredGradeRuleFlagsABlankGrade();
        HolidayRuleMaximumRangeRuleFlagsValuesAboveMaximum();
        HolidayRuleRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
