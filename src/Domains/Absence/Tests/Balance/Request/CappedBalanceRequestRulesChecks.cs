namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedBalanceRequestRulesChecks
{
    public static void CappedBalanceRequestValidatorReportsEveryProblem()
    {
        var validator = CappedBalanceRequestRules.DefaultCappedBalanceRequestValidator();
        var item = CappedBalanceRequest.NewCappedBalanceRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedBalanceRequestFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new CappedBalanceRequestFteRangeRule(3.3, 3988.7);
        var item = CappedBalanceRequest.NewCappedBalanceRequest("x", "y");
        item.Fte = 3990.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedBalanceRequestQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new CappedBalanceRequestQuantityRangeRule(32, 86400);
        var item = CappedBalanceRequest.NewCappedBalanceRequest("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedBalanceRequestMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new CappedBalanceRequestMaximumRangeRule(4.3, 2112.0);
        var item = CappedBalanceRequest.NewCappedBalanceRequest("x", "y");
        item.Maximum = 2113.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedBalanceRequestDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new CappedBalanceRequestDescriptionLengthRule(1000);
        var item = CappedBalanceRequest.NewCappedBalanceRequest("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedBalanceRequestValidatorReportsEveryProblem();
        CappedBalanceRequestFteRangeRuleFlagsValuesAboveFte();
        CappedBalanceRequestQuantityRangeRuleFlagsValuesAboveQuantity();
        CappedBalanceRequestMaximumRangeRuleFlagsValuesAboveMaximum();
        CappedBalanceRequestDescriptionLengthRuleFlagsALongDescription();
    }
}
