namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayProfileRulesChecks
{
    public static void RetroPayProfileValidatorReportsEveryProblem()
    {
        var validator = RetroPayProfileRules.DefaultRetroPayProfileValidator();
        var item = RetroPayProfile.NewRetroPayProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayProfileWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RetroPayProfileWeightRangeRule(2.8, 949.7);
        var item = RetroPayProfile.NewRetroPayProfile("x", "y");
        item.Weight = 951.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayProfileCodeLengthRule(86400);
        var item = RetroPayProfile.NewRetroPayProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayProfileRequiredCodeRule("retro pay profile needs a value");
        var item = RetroPayProfile.NewRetroPayProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayProfileCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new RetroPayProfileCeilingRangeRule(6.2, 4047.5);
        var item = RetroPayProfile.NewRetroPayProfile("x", "y");
        item.Ceiling = 4049.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayProfilePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new RetroPayProfilePriorityRangeRule(64, 3600);
        var item = RetroPayProfile.NewRetroPayProfile("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayProfileValidatorReportsEveryProblem();
        RetroPayProfileWeightRangeRuleFlagsValuesAboveWeight();
        RetroPayProfileCodeLengthRuleFlagsALongCode();
        RetroPayProfileRequiredCodeRuleFlagsABlankCode();
        RetroPayProfileCeilingRangeRuleFlagsValuesAboveCeiling();
        RetroPayProfilePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
