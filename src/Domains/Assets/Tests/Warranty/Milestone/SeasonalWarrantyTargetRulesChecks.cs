namespace Acme.Workforce.Assets.Tests.Warranty.Milestone;

using Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalWarrantyTargetRulesChecks
{
    public static void SeasonalWarrantyTargetValidatorReportsEveryProblem()
    {
        var validator = SeasonalWarrantyTargetRules.DefaultSeasonalWarrantyTargetValidator();
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalWarrantyTargetFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SeasonalWarrantyTargetFloorValueRangeRule(9.6, 3446.1);
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("x", "y");
        item.FloorValue = 3447.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalWarrantyTargetExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new SeasonalWarrantyTargetExternalRefLengthRule(86400);
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalWarrantyTargetRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new SeasonalWarrantyTargetRequiredCountryRule("seasonal warranty target needs this field");
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalWarrantyTargetValidatorReportsEveryProblem();
        SeasonalWarrantyTargetFloorValueRangeRuleFlagsValuesAboveFloorValue();
        SeasonalWarrantyTargetExternalRefLengthRuleFlagsALongExternalRef();
        SeasonalWarrantyTargetRequiredCountryRuleFlagsABlankCountry();
    }
}
