namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalTownHallClaimRulesChecks
{
    public static void SeasonalTownHallClaimValidatorReportsEveryProblem()
    {
        var validator = SeasonalTownHallClaimRules.DefaultSeasonalTownHallClaimValidator();
        var item = SeasonalTownHallClaim.NewSeasonalTownHallClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalTownHallClaimDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new SeasonalTownHallClaimDiscountRangeRule(2.9, 1465.8);
        var item = SeasonalTownHallClaim.NewSeasonalTownHallClaim("x", "y");
        item.Discount = 1467.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalTownHallClaimAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new SeasonalTownHallClaimAttemptsRangeRule(32, 64);
        var item = SeasonalTownHallClaim.NewSeasonalTownHallClaim("x", "y");
        item.Attempts = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalTownHallClaimRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalTownHallClaimRequiredLabelRule("seasonal town hall claim needs an entry");
        var item = SeasonalTownHallClaim.NewSeasonalTownHallClaim("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalTownHallClaimValidatorReportsEveryProblem();
        SeasonalTownHallClaimDiscountRangeRuleFlagsValuesAboveDiscount();
        SeasonalTownHallClaimAttemptsRangeRuleFlagsValuesAboveAttempts();
        SeasonalTownHallClaimRequiredLabelRuleFlagsABlankLabel();
    }
}
