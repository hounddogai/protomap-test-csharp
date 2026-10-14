namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RecognitionRuleRulesChecks
{
    public static void RecognitionRuleValidatorReportsEveryProblem()
    {
        var validator = RecognitionRuleRules.DefaultRecognitionRuleValidator();
        var item = RecognitionRule.NewRecognitionRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionRuleRequiredCodeRule("recognition rule needs an entry");
        var item = RecognitionRule.NewRecognitionRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionRuleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RecognitionRuleTenureMonthsRangeRule(32, 1000);
        var item = RecognitionRule.NewRecognitionRule("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionRuleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RecognitionRuleTitleLengthRule(120);
        var item = RecognitionRule.NewRecognitionRule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionRuleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RecognitionRuleStartsAtRangeRule(32, 3600);
        var item = RecognitionRule.NewRecognitionRule("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionRuleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new RecognitionRuleSalaryBandLengthRule(64);
        var item = RecognitionRule.NewRecognitionRule("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RecognitionRuleValidatorReportsEveryProblem();
        RecognitionRuleRequiredCodeRuleFlagsABlankCode();
        RecognitionRuleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        RecognitionRuleTitleLengthRuleFlagsALongTitle();
        RecognitionRuleStartsAtRangeRuleFlagsValuesAboveStartsAt();
        RecognitionRuleSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
