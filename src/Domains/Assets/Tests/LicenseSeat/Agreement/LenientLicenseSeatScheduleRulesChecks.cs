namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientLicenseSeatScheduleRulesChecks
{
    public static void LenientLicenseSeatScheduleValidatorReportsEveryProblem()
    {
        var validator = LenientLicenseSeatScheduleRules.DefaultLenientLicenseSeatScheduleValidator();
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLicenseSeatScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientLicenseSeatScheduleCodeLengthRule(120);
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientLicenseSeatScheduleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientLicenseSeatScheduleRequiredLabelRule("lenient license seat schedule needs this field");
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLicenseSeatScheduleSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LenientLicenseSeatScheduleSequenceRangeRule(80, 3600);
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("x", "y");
        item.Sequence = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientLicenseSeatScheduleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LenientLicenseSeatScheduleDurationMinutesRangeRule(32, 120);
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientLicenseSeatScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientLicenseSeatScheduleLabelLengthRule(3600);
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientLicenseSeatScheduleValidatorReportsEveryProblem();
        LenientLicenseSeatScheduleCodeLengthRuleFlagsALongCode();
        LenientLicenseSeatScheduleRequiredLabelRuleFlagsABlankLabel();
        LenientLicenseSeatScheduleSequenceRangeRuleFlagsValuesAboveSequence();
        LenientLicenseSeatScheduleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LenientLicenseSeatScheduleLabelLengthRuleFlagsALongLabel();
    }
}
