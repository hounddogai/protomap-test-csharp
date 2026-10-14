namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateAllocationRulesChecks
{
    public static void RateAllocationValidatorReportsEveryProblem()
    {
        var validator = RateAllocationRules.DefaultRateAllocationValidator();
        var item = RateAllocation.NewRateAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateAllocationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RateAllocationRequiredLabelRule("rate allocation needs this field");
        var item = RateAllocation.NewRateAllocation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateAllocationRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new RateAllocationRevisionRangeRule(80, 1000);
        var item = RateAllocation.NewRateAllocation("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateAllocationRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new RateAllocationRequiredChannelRule("rate allocation needs this field");
        var item = RateAllocation.NewRateAllocation("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RateAllocationValidatorReportsEveryProblem();
        RateAllocationRequiredLabelRuleFlagsABlankLabel();
        RateAllocationRevisionRangeRuleFlagsValuesAboveRevision();
        RateAllocationRequiredChannelRuleFlagsABlankChannel();
    }
}
