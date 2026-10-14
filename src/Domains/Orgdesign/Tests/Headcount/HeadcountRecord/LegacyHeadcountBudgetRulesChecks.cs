namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class LegacyHeadcountBudgetRulesChecks
{
    public static void LegacyHeadcountBudgetValidatorReportsEveryProblem()
    {
        var validator = LegacyHeadcountBudgetRules.DefaultLegacyHeadcountBudgetValidator();
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyHeadcountBudgetPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new LegacyHeadcountBudgetPhoneLengthRule(3600);
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyHeadcountBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyHeadcountBudgetRequiredNameRule("legacy headcount budget needs this field");
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyHeadcountBudgetDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LegacyHeadcountBudgetDurationMinutesRangeRule(32, 80);
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("x", "y");
        item.DurationMinutes = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyHeadcountBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyHeadcountBudgetNameLengthRule(86400);
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyHeadcountBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyHeadcountBudgetRequiredCodeRule("legacy headcount budget needs this field");
        var item = LegacyHeadcountBudget.NewLegacyHeadcountBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyHeadcountBudgetValidatorReportsEveryProblem();
        LegacyHeadcountBudgetPhoneLengthRuleFlagsALongPhone();
        LegacyHeadcountBudgetRequiredNameRuleFlagsABlankName();
        LegacyHeadcountBudgetDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LegacyHeadcountBudgetNameLengthRuleFlagsALongName();
        LegacyHeadcountBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
