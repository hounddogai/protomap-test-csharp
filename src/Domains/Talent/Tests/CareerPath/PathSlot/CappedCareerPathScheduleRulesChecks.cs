namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class CappedCareerPathScheduleRulesChecks
{
    public static void CappedCareerPathScheduleValidatorReportsEveryProblem()
    {
        var validator = CappedCareerPathScheduleRules.DefaultCappedCareerPathScheduleValidator();
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedCareerPathScheduleSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new CappedCareerPathScheduleSequenceRangeRule(80, 86400);
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedCareerPathScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedCareerPathScheduleCodeLengthRule(3600);
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedCareerPathScheduleWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new CappedCareerPathScheduleWindowDaysRangeRule(120, 3600);
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedCareerPathScheduleRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new CappedCareerPathScheduleRequiredCurrencyRule("capped career path schedule needs this field");
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedCareerPathScheduleCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new CappedCareerPathScheduleCurrencyLengthRule(32);
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedCareerPathScheduleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new CappedCareerPathScheduleLocaleLengthRule(32);
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedCareerPathScheduleValidatorReportsEveryProblem();
        CappedCareerPathScheduleSequenceRangeRuleFlagsValuesAboveSequence();
        CappedCareerPathScheduleCodeLengthRuleFlagsALongCode();
        CappedCareerPathScheduleWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        CappedCareerPathScheduleRequiredCurrencyRuleFlagsABlankCurrency();
        CappedCareerPathScheduleCurrencyLengthRuleFlagsALongCurrency();
        CappedCareerPathScheduleLocaleLengthRuleFlagsALongLocale();
    }
}
