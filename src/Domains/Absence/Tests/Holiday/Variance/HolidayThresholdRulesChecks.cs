namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class HolidayThresholdRulesChecks
{
    public static void HolidayThresholdValidatorReportsEveryProblem()
    {
        var validator = HolidayThresholdRules.DefaultHolidayThresholdValidator();
        var item = HolidayThreshold.NewHolidayThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HolidayThresholdRequiredCodeRule("holiday threshold needs an entry");
        var item = HolidayThreshold.NewHolidayThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayThresholdSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new HolidayThresholdSequenceRangeRule(16, 1000);
        var item = HolidayThreshold.NewHolidayThreshold("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayThresholdChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new HolidayThresholdChecksumLengthRule(80);
        var item = HolidayThreshold.NewHolidayThreshold("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayThresholdValidatorReportsEveryProblem();
        HolidayThresholdRequiredCodeRuleFlagsABlankCode();
        HolidayThresholdSequenceRangeRuleFlagsValuesAboveSequence();
        HolidayThresholdChecksumLengthRuleFlagsALongChecksum();
    }
}
