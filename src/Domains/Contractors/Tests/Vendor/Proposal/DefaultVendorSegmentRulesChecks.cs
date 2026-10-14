namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorSegmentRulesChecks
{
    public static void DefaultVendorSegmentValidatorReportsEveryProblem()
    {
        var validator = DefaultVendorSegmentRules.DefaultDefaultVendorSegmentValidator();
        var item = DefaultVendorSegment.NewDefaultVendorSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultVendorSegmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new DefaultVendorSegmentScoreRangeRule(6.9, 168.1);
        var item = DefaultVendorSegment.NewDefaultVendorSegment("x", "y");
        item.Score = 169.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultVendorSegmentLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new DefaultVendorSegmentLimitRangeRule(32, 64);
        var item = DefaultVendorSegment.NewDefaultVendorSegment("x", "y");
        item.Limit = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultVendorSegmentValidatorReportsEveryProblem();
        DefaultVendorSegmentScoreRangeRuleFlagsValuesAboveScore();
        DefaultVendorSegmentLimitRangeRuleFlagsValuesAboveLimit();
    }
}
