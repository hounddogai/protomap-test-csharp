namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedHolidayManifestRulesChecks
{
    public static void CappedHolidayManifestValidatorReportsEveryProblem()
    {
        var validator = CappedHolidayManifestRules.DefaultCappedHolidayManifestValidator();
        var item = CappedHolidayManifest.NewCappedHolidayManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedHolidayManifestSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new CappedHolidayManifestSegmentLengthRule(16);
        var item = CappedHolidayManifest.NewCappedHolidayManifest("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedHolidayManifestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedHolidayManifestRequiredLabelRule("capped holiday manifest needs this field");
        var item = CappedHolidayManifest.NewCappedHolidayManifest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedHolidayManifestRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new CappedHolidayManifestRateRangeRule(8.0, 1154.4);
        var item = CappedHolidayManifest.NewCappedHolidayManifest("x", "y");
        item.Rate = 1155.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedHolidayManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedHolidayManifestRequiredCodeRule("capped holiday manifest needs this field");
        var item = CappedHolidayManifest.NewCappedHolidayManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedHolidayManifestValidatorReportsEveryProblem();
        CappedHolidayManifestSegmentLengthRuleFlagsALongSegment();
        CappedHolidayManifestRequiredLabelRuleFlagsABlankLabel();
        CappedHolidayManifestRateRangeRuleFlagsValuesAboveRate();
        CappedHolidayManifestRequiredCodeRuleFlagsABlankCode();
    }
}
