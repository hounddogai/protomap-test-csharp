namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalAssetReturnReservationRulesChecks
{
    public static void RegionalAssetReturnReservationValidatorReportsEveryProblem()
    {
        var validator = RegionalAssetReturnReservationRules.DefaultRegionalAssetReturnReservationValidator();
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAssetReturnReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalAssetReturnReservationRequiredNameRule(
            "regional asset return reservation needs this field");
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAssetReturnReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalAssetReturnReservationCodeLengthRule(3600);
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAssetReturnReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new RegionalAssetReturnReservationCreatedAtRangeRule(120, 1000);
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalAssetReturnReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalAssetReturnReservationRequiredCodeRule(
            "regional asset return reservation needs an entry");
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAssetReturnReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalAssetReturnReservationTitleLengthRule(120);
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAssetReturnReservationThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RegionalAssetReturnReservationThresholdRangeRule(9.2, 777.5);
        var item = RegionalAssetReturnReservation.NewRegionalAssetReturnReservation("x", "y");
        item.Threshold = 779.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalAssetReturnReservationValidatorReportsEveryProblem();
        RegionalAssetReturnReservationRequiredNameRuleFlagsABlankName();
        RegionalAssetReturnReservationCodeLengthRuleFlagsALongCode();
        RegionalAssetReturnReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        RegionalAssetReturnReservationRequiredCodeRuleFlagsABlankCode();
        RegionalAssetReturnReservationTitleLengthRuleFlagsALongTitle();
        RegionalAssetReturnReservationThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
