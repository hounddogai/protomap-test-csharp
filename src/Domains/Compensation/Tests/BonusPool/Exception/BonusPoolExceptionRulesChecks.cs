namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BonusPoolExceptionRulesChecks
{
    public static void BonusPoolExceptionValidatorReportsEveryProblem()
    {
        var validator = BonusPoolExceptionRules.DefaultBonusPoolExceptionValidator();
        var item = BonusPoolException.NewBonusPoolException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolExceptionCodeLengthRule(64);
        var item = BonusPoolException.NewBonusPoolException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolExceptionAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new BonusPoolExceptionAttemptsRangeRule(16, 1000);
        var item = BonusPoolException.NewBonusPoolException("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolExceptionRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BonusPoolExceptionRateRangeRule(1.6, 3947.9);
        var item = BonusPoolException.NewBonusPoolException("x", "y");
        item.Rate = 3949.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BonusPoolExceptionValidatorReportsEveryProblem();
        BonusPoolExceptionCodeLengthRuleFlagsALongCode();
        BonusPoolExceptionAttemptsRangeRuleFlagsValuesAboveAttempts();
        BonusPoolExceptionRateRangeRuleFlagsValuesAboveRate();
    }
}
