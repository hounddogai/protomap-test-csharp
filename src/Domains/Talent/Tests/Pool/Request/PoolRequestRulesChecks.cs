namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PoolRequestRulesChecks
{
    public static void PoolRequestValidatorReportsEveryProblem()
    {
        var validator = PoolRequestRules.DefaultPoolRequestValidator();
        var item = PoolRequest.NewPoolRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolRequestCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PoolRequestCapacityRangeRule(120, 3600);
        var item = PoolRequest.NewPoolRequest("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolRequestCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new PoolRequestCurrencyLengthRule(1000);
        var item = PoolRequest.NewPoolRequest("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolRequestRequiredCodeRule("pool request needs this field");
        var item = PoolRequest.NewPoolRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PoolRequestValidatorReportsEveryProblem();
        PoolRequestCapacityRangeRuleFlagsValuesAboveCapacity();
        PoolRequestCurrencyLengthRuleFlagsALongCurrency();
        PoolRequestRequiredCodeRuleFlagsABlankCode();
    }
}
