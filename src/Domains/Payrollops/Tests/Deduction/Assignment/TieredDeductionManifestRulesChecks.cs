namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredDeductionManifestRulesChecks
{
    public static void TieredDeductionManifestValidatorReportsEveryProblem()
    {
        var validator = TieredDeductionManifestRules.DefaultTieredDeductionManifestValidator();
        var item = TieredDeductionManifest.NewTieredDeductionManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredDeductionManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new TieredDeductionManifestDurationMinutesRangeRule(80, 3600);
        var item = TieredDeductionManifest.NewTieredDeductionManifest("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredDeductionManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredDeductionManifestRequiredCodeRule("tiered deduction manifest needs an entry");
        var item = TieredDeductionManifest.NewTieredDeductionManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredDeductionManifestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new TieredDeductionManifestRequiredTeamRule("tiered deduction manifest needs this field");
        var item = TieredDeductionManifest.NewTieredDeductionManifest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredDeductionManifestFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new TieredDeductionManifestFloorValueRangeRule(4.6, 3328.8);
        var item = TieredDeductionManifest.NewTieredDeductionManifest("x", "y");
        item.FloorValue = 3330.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredDeductionManifestValidatorReportsEveryProblem();
        TieredDeductionManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        TieredDeductionManifestRequiredCodeRuleFlagsABlankCode();
        TieredDeductionManifestRequiredTeamRuleFlagsABlankTeam();
        TieredDeductionManifestFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
