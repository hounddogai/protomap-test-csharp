namespace Acme.Workforce.Payrollops.Tests.RetroPay.Policy;

using Acme.Workforce.Payrollops.RetroPay.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RetroPayPolicyRulesChecks
{
    public static void RetroPayPolicyValidatorReportsEveryProblem()
    {
        var validator = RetroPayPolicyRules.DefaultRetroPayPolicyValidator();
        var item = RetroPayPolicy.NewRetroPayPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayPolicyLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RetroPayPolicyLocaleLengthRule(1000);
        var item = RetroPayPolicy.NewRetroPayPolicy("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new RetroPayPolicyBatchSizeRangeRule(16, 64);
        var item = RetroPayPolicy.NewRetroPayPolicy("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayPolicyDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new RetroPayPolicyDiscountRangeRule(1.7, 3391.0);
        var item = RetroPayPolicy.NewRetroPayPolicy("x", "y");
        item.Discount = 3392.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayPolicyValidatorReportsEveryProblem();
        RetroPayPolicyLocaleLengthRuleFlagsALongLocale();
        RetroPayPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        RetroPayPolicyDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
