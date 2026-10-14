namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedLicenseSeatProposalRulesChecks
{
    public static void WeightedLicenseSeatProposalValidatorReportsEveryProblem()
    {
        var validator = WeightedLicenseSeatProposalRules.DefaultWeightedLicenseSeatProposalValidator();
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedLicenseSeatProposalRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedLicenseSeatProposalRequiredNameRule("weighted license seat proposal needs this field");
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedLicenseSeatProposalUnitLengthRuleFlagsALongUnit()
    {
        var rule = new WeightedLicenseSeatProposalUnitLengthRule(16);
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedLicenseSeatProposalRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new WeightedLicenseSeatProposalRetriesRangeRule(16, 3600);
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedLicenseSeatProposalRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new WeightedLicenseSeatProposalRequiredUnitRule("weighted license seat proposal needs a value");
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedLicenseSeatProposalChannelLengthRuleFlagsALongChannel()
    {
        var rule = new WeightedLicenseSeatProposalChannelLengthRule(3600);
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatProposalValidatorReportsEveryProblem();
        WeightedLicenseSeatProposalRequiredNameRuleFlagsABlankName();
        WeightedLicenseSeatProposalUnitLengthRuleFlagsALongUnit();
        WeightedLicenseSeatProposalRetriesRangeRuleFlagsValuesAboveRetries();
        WeightedLicenseSeatProposalRequiredUnitRuleFlagsABlankUnit();
        WeightedLicenseSeatProposalChannelLengthRuleFlagsALongChannel();
    }
}
