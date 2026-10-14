namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientBonusPoolPolicyRulesChecks
{
    public static void LenientBonusPoolPolicyValidatorReportsEveryProblem()
    {
        var validator = LenientBonusPoolPolicyRules.DefaultLenientBonusPoolPolicyValidator();
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBonusPoolPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientBonusPoolPolicyCodeLengthRule(255);
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBonusPoolPolicyRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientBonusPoolPolicyRequiredNameRule("lenient bonus pool policy needs an entry");
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBonusPoolPolicyLocationLengthRuleFlagsALongLocation()
    {
        var rule = new LenientBonusPoolPolicyLocationLengthRule(16);
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientBonusPoolPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientBonusPoolPolicyRequiredCodeRule("lenient bonus pool policy needs a value");
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBonusPoolPolicyHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new LenientBonusPoolPolicyHoursRangeRule(5.2, 993.4);
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("x", "y");
        item.Hours = 994.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientBonusPoolPolicyValidatorReportsEveryProblem();
        LenientBonusPoolPolicyCodeLengthRuleFlagsALongCode();
        LenientBonusPoolPolicyRequiredNameRuleFlagsABlankName();
        LenientBonusPoolPolicyLocationLengthRuleFlagsALongLocation();
        LenientBonusPoolPolicyRequiredCodeRuleFlagsABlankCode();
        LenientBonusPoolPolicyHoursRangeRuleFlagsValuesAboveHours();
    }
}
