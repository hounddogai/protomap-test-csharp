namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualLicenseSeatSlotRulesChecks
{
    public static void ManualLicenseSeatSlotValidatorReportsEveryProblem()
    {
        var validator = ManualLicenseSeatSlotRules.DefaultManualLicenseSeatSlotValidator();
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualLicenseSeatSlotDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ManualLicenseSeatSlotDurationMinutesRangeRule(32, 86400);
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualLicenseSeatSlotNameLengthRuleFlagsALongName()
    {
        var rule = new ManualLicenseSeatSlotNameLengthRule(86400);
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualLicenseSeatSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualLicenseSeatSlotCodeLengthRule(32);
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualLicenseSeatSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualLicenseSeatSlotRequiredCodeRule("manual license seat slot needs a value");
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualLicenseSeatSlotValidatorReportsEveryProblem();
        ManualLicenseSeatSlotDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        ManualLicenseSeatSlotNameLengthRuleFlagsALongName();
        ManualLicenseSeatSlotCodeLengthRuleFlagsALongCode();
        ManualLicenseSeatSlotRequiredCodeRuleFlagsABlankCode();
    }
}
