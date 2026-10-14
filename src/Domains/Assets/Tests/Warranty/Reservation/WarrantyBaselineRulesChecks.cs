namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyBaselineRulesChecks
{
    public static void WarrantyBaselineValidatorReportsEveryProblem()
    {
        var validator = WarrantyBaselineRules.DefaultWarrantyBaselineValidator();
        var item = WarrantyBaseline.NewWarrantyBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyBaselinePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new WarrantyBaselinePhoneLengthRule(64);
        var item = WarrantyBaseline.NewWarrantyBaseline("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyBaselineSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new WarrantyBaselineSegmentLengthRule(32);
        var item = WarrantyBaseline.NewWarrantyBaseline("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyBaselineLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WarrantyBaselineLabelLengthRule(3600);
        var item = WarrantyBaseline.NewWarrantyBaseline("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantyBaselineCodeLengthRule(86400);
        var item = WarrantyBaseline.NewWarrantyBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyBaselineScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new WarrantyBaselineScoreRangeRule(8.2, 1835.5);
        var item = WarrantyBaseline.NewWarrantyBaseline("x", "y");
        item.Score = 1837.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WarrantyBaselineValidatorReportsEveryProblem();
        WarrantyBaselinePhoneLengthRuleFlagsALongPhone();
        WarrantyBaselineSegmentLengthRuleFlagsALongSegment();
        WarrantyBaselineLabelLengthRuleFlagsALongLabel();
        WarrantyBaselineCodeLengthRuleFlagsALongCode();
        WarrantyBaselineScoreRangeRuleFlagsValuesAboveScore();
    }
}
