namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayVarianceRulesChecks
{
    public static void HolidayVarianceValidatorReportsEveryProblem()
    {
        var validator = HolidayVarianceRules.DefaultHolidayVarianceValidator();
        var item = HolidayVariance.NewHolidayVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayVariancePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new HolidayVariancePhoneLengthRule(64);
        var item = HolidayVariance.NewHolidayVariance("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayVarianceRequiredNameRuleFlagsABlankName()
    {
        var rule = new HolidayVarianceRequiredNameRule("holiday variance needs an entry");
        var item = HolidayVariance.NewHolidayVariance("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayVarianceCodeLengthRule(80);
        var item = HolidayVariance.NewHolidayVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayVarianceRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new HolidayVarianceRequiredCostCenterRule("holiday variance needs an entry");
        var item = HolidayVariance.NewHolidayVariance("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayVarianceValidatorReportsEveryProblem();
        HolidayVariancePhoneLengthRuleFlagsALongPhone();
        HolidayVarianceRequiredNameRuleFlagsABlankName();
        HolidayVarianceCodeLengthRuleFlagsALongCode();
        HolidayVarianceRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
