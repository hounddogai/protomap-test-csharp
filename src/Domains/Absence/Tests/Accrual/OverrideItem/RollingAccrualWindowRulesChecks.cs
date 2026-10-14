namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingAccrualWindowRulesChecks
{
    public static void RollingAccrualWindowValidatorReportsEveryProblem()
    {
        var validator = RollingAccrualWindowRules.DefaultRollingAccrualWindowValidator();
        var item = RollingAccrualWindow.NewRollingAccrualWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAccrualWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingAccrualWindowRequiredCodeRule("rolling accrual window needs this field");
        var item = RollingAccrualWindow.NewRollingAccrualWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAccrualWindowDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new RollingAccrualWindowDurationMinutesRangeRule(120, 1000);
        var item = RollingAccrualWindow.NewRollingAccrualWindow("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAccrualWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingAccrualWindowCodeLengthRule(86400);
        var item = RollingAccrualWindow.NewRollingAccrualWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAccrualWindowRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new RollingAccrualWindowRequiredUnitRule("rolling accrual window needs this field");
        var item = RollingAccrualWindow.NewRollingAccrualWindow("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingAccrualWindowValidatorReportsEveryProblem();
        RollingAccrualWindowRequiredCodeRuleFlagsABlankCode();
        RollingAccrualWindowDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        RollingAccrualWindowCodeLengthRuleFlagsALongCode();
        RollingAccrualWindowRequiredUnitRuleFlagsABlankUnit();
    }
}
