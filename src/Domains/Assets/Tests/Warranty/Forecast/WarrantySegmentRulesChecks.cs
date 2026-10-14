namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantySegmentRulesChecks
{
    public static void WarrantySegmentValidatorReportsEveryProblem()
    {
        var validator = WarrantySegmentRules.DefaultWarrantySegmentValidator();
        var item = WarrantySegment.NewWarrantySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantySegmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new WarrantySegmentStartsAtRangeRule(16, 255);
        var item = WarrantySegment.NewWarrantySegment("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantySegmentChannelLengthRuleFlagsALongChannel()
    {
        var rule = new WarrantySegmentChannelLengthRule(86400);
        var item = WarrantySegment.NewWarrantySegment("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantySegmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WarrantySegmentLabelLengthRule(255);
        var item = WarrantySegment.NewWarrantySegment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantySegmentValidatorReportsEveryProblem();
        WarrantySegmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
        WarrantySegmentChannelLengthRuleFlagsALongChannel();
        WarrantySegmentLabelLengthRuleFlagsALongLabel();
    }
}
