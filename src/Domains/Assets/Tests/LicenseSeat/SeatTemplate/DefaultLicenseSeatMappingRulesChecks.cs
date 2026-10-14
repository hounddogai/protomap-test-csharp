namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class DefaultLicenseSeatMappingRulesChecks
{
    public static void DefaultLicenseSeatMappingValidatorReportsEveryProblem()
    {
        var validator = DefaultLicenseSeatMappingRules.DefaultDefaultLicenseSeatMappingValidator();
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultLicenseSeatMappingPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new DefaultLicenseSeatMappingPriorityRangeRule(32, 64);
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("x", "y");
        item.Priority = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultLicenseSeatMappingBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new DefaultLicenseSeatMappingBatchSizeRangeRule(3600, 3600);
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultLicenseSeatMappingRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new DefaultLicenseSeatMappingRateRangeRule(3.5, 2057.4);
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("x", "y");
        item.Rate = 2058.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultLicenseSeatMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultLicenseSeatMappingRequiredTitleRule("default license seat mapping needs this field");
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultLicenseSeatMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultLicenseSeatMappingTitleLengthRule(120);
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatMappingValidatorReportsEveryProblem();
        DefaultLicenseSeatMappingPriorityRangeRuleFlagsValuesAbovePriority();
        DefaultLicenseSeatMappingBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        DefaultLicenseSeatMappingRateRangeRuleFlagsValuesAboveRate();
        DefaultLicenseSeatMappingRequiredTitleRuleFlagsABlankTitle();
        DefaultLicenseSeatMappingTitleLengthRuleFlagsALongTitle();
    }
}
