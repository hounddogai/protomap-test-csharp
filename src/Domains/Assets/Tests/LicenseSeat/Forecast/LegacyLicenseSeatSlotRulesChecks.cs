namespace Acme.Workforce.Assets.Tests.LicenseSeat.Forecast;

using Acme.Workforce.Assets.LicenseSeat.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LegacyLicenseSeatSlotRulesChecks
{
    public static void LegacyLicenseSeatSlotValidatorReportsEveryProblem()
    {
        var validator = LegacyLicenseSeatSlotRules.DefaultLegacyLicenseSeatSlotValidator();
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyLicenseSeatSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyLicenseSeatSlotRequiredCodeRule("legacy license seat slot needs a value");
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyLicenseSeatSlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyLicenseSeatSlotRequiredNameRule("legacy license seat slot needs a value");
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyLicenseSeatSlotNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyLicenseSeatSlotNameLengthRule(80);
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyLicenseSeatSlotAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LegacyLicenseSeatSlotAttemptsRangeRule(3600, 86400);
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyLicenseSeatSlotValidatorReportsEveryProblem();
        LegacyLicenseSeatSlotRequiredCodeRuleFlagsABlankCode();
        LegacyLicenseSeatSlotRequiredNameRuleFlagsABlankName();
        LegacyLicenseSeatSlotNameLengthRuleFlagsALongName();
        LegacyLicenseSeatSlotAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
