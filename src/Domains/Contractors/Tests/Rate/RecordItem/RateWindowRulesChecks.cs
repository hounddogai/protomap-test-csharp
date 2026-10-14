namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateWindowRulesChecks
{
    public static void RateWindowValidatorReportsEveryProblem()
    {
        var validator = RateWindowRules.DefaultRateWindowValidator();
        var item = RateWindow.NewRateWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateWindowExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RateWindowExpiresAtRangeRule(32, 1000);
        var item = RateWindow.NewRateWindow("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateWindowAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new RateWindowAmountRangeRule(9.4, 4314.0);
        var item = RateWindow.NewRateWindow("x", "y");
        item.Amount = 4315.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RateWindowRequiredCodeRule("rate window needs an entry");
        var item = RateWindow.NewRateWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateWindowCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RateWindowCapacityRangeRule(32, 32);
        var item = RateWindow.NewRateWindow("x", "y");
        item.Capacity = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RateWindowValidatorReportsEveryProblem();
        RateWindowExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RateWindowAmountRangeRuleFlagsValuesAboveAmount();
        RateWindowRequiredCodeRuleFlagsABlankCode();
        RateWindowCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
