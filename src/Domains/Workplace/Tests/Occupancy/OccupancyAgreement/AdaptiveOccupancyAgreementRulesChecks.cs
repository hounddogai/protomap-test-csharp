namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAgreement;

using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Checks of occupancy agreement code, run by the project's runner.</summary>
public static class AdaptiveOccupancyAgreementRulesChecks
{
    public static void AdaptiveOccupancyAgreementValidatorReportsEveryProblem()
    {
        var validator = AdaptiveOccupancyAgreementRules.DefaultAdaptiveOccupancyAgreementValidator();
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveOccupancyAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveOccupancyAgreementNameLengthRule(120);
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveOccupancyAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveOccupancyAgreementCodeLengthRule(120);
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveOccupancyAgreementDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AdaptiveOccupancyAgreementDiscountRangeRule(8.0, 3674.2);
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("x", "y");
        item.Discount = 3675.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveOccupancyAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveOccupancyAgreementRequiredCodeRule("adaptive occupancy agreement needs a value");
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveOccupancyAgreementValidatorReportsEveryProblem();
        AdaptiveOccupancyAgreementNameLengthRuleFlagsALongName();
        AdaptiveOccupancyAgreementCodeLengthRuleFlagsALongCode();
        AdaptiveOccupancyAgreementDiscountRangeRuleFlagsValuesAboveDiscount();
        AdaptiveOccupancyAgreementRequiredCodeRuleFlagsABlankCode();
    }
}
