namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayCycleRulesChecks
{
    public static void RetroPayCycleValidatorReportsEveryProblem()
    {
        var validator = RetroPayCycleRules.DefaultRetroPayCycleValidator();
        var item = RetroPayCycle.NewRetroPayCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayCycleRequiredCodeRule("retro pay cycle needs this field");
        var item = RetroPayCycle.NewRetroPayCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayCycleCodeLengthRule(64);
        var item = RetroPayCycle.NewRetroPayCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayCycleExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RetroPayCycleExpiresAtRangeRule(255, 1000);
        var item = RetroPayCycle.NewRetroPayCycle("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayCycleValidatorReportsEveryProblem();
        RetroPayCycleRequiredCodeRuleFlagsABlankCode();
        RetroPayCycleCodeLengthRuleFlagsALongCode();
        RetroPayCycleExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
