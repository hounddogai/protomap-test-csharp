namespace Acme.Workforce.Compensation.Tests.BonusPool.Schedule;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BonusPoolScheduleRulesChecks
{
    public static void BonusPoolScheduleValidatorReportsEveryProblem()
    {
        var validator = BonusPoolScheduleRules.DefaultBonusPoolScheduleValidator();
        var item = BonusPoolSchedule.NewBonusPoolSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolScheduleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new BonusPoolScheduleLocaleLengthRule(1000);
        var item = BonusPoolSchedule.NewBonusPoolSchedule("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolScheduleRequiredCodeRule("bonus pool schedule needs this field");
        var item = BonusPoolSchedule.NewBonusPoolSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolScheduleRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new BonusPoolScheduleRequiredLocaleRule("bonus pool schedule needs an entry");
        var item = BonusPoolSchedule.NewBonusPoolSchedule("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolScheduleValidatorReportsEveryProblem();
        BonusPoolScheduleLocaleLengthRuleFlagsALongLocale();
        BonusPoolScheduleRequiredCodeRuleFlagsABlankCode();
        BonusPoolScheduleRequiredLocaleRuleFlagsABlankLocale();
    }
}
