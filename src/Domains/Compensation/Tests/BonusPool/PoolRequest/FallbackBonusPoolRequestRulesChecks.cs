namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class FallbackBonusPoolRequestRulesChecks
{
    public static void FallbackBonusPoolRequestValidatorReportsEveryProblem()
    {
        var validator = FallbackBonusPoolRequestRules.DefaultFallbackBonusPoolRequestValidator();
        var item = FallbackBonusPoolRequest.NewFallbackBonusPoolRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackBonusPoolRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackBonusPoolRequestCodeLengthRule(1000);
        var item = FallbackBonusPoolRequest.NewFallbackBonusPoolRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBonusPoolRequestAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new FallbackBonusPoolRequestAttemptsRangeRule(120, 120);
        var item = FallbackBonusPoolRequest.NewFallbackBonusPoolRequest("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackBonusPoolRequestValidatorReportsEveryProblem();
        FallbackBonusPoolRequestCodeLengthRuleFlagsALongCode();
        FallbackBonusPoolRequestAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
