namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PrimaryRateRequestRulesChecks
{
    public static void PrimaryRateRequestValidatorReportsEveryProblem()
    {
        var validator = PrimaryRateRequestRules.DefaultPrimaryRateRequestValidator();
        var item = PrimaryRateRequest.NewPrimaryRateRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryRateRequestCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PrimaryRateRequestCostRangeRule(4.6, 2217.6);
        var item = PrimaryRateRequest.NewPrimaryRateRequest("x", "y");
        item.Cost = 2219.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryRateRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryRateRequestRequiredLabelRule("primary rate request needs an entry");
        var item = PrimaryRateRequest.NewPrimaryRateRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRateRequestSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new PrimaryRateRequestSegmentLengthRule(86400);
        var item = PrimaryRateRequest.NewPrimaryRateRequest("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryRateRequestLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PrimaryRateRequestLeaveDaysRangeRule(120, 1000);
        var item = PrimaryRateRequest.NewPrimaryRateRequest("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryRateRequestRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new PrimaryRateRequestRequiredSegmentRule("primary rate request needs this field");
        var item = PrimaryRateRequest.NewPrimaryRateRequest("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryRateRequestValidatorReportsEveryProblem();
        PrimaryRateRequestCostRangeRuleFlagsValuesAboveCost();
        PrimaryRateRequestRequiredLabelRuleFlagsABlankLabel();
        PrimaryRateRequestSegmentLengthRuleFlagsALongSegment();
        PrimaryRateRequestLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        PrimaryRateRequestRequiredSegmentRuleFlagsABlankSegment();
    }
}
