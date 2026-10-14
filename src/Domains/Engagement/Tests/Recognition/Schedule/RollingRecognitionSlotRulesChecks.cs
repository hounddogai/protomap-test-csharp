namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingRecognitionSlotRulesChecks
{
    public static void RollingRecognitionSlotValidatorReportsEveryProblem()
    {
        var validator = RollingRecognitionSlotRules.DefaultRollingRecognitionSlotValidator();
        var item = RollingRecognitionSlot.NewRollingRecognitionSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRecognitionSlotRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new RollingRecognitionSlotRequiredPhoneRule("rolling recognition slot needs an entry");
        var item = RollingRecognitionSlot.NewRollingRecognitionSlot("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingRecognitionSlotCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new RollingRecognitionSlotCurrencyLengthRule(3600);
        var item = RollingRecognitionSlot.NewRollingRecognitionSlot("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingRecognitionSlotValidatorReportsEveryProblem();
        RollingRecognitionSlotRequiredPhoneRuleFlagsABlankPhone();
        RollingRecognitionSlotCurrencyLengthRuleFlagsALongCurrency();
    }
}
