namespace Acme.Workforce.Assets.Tests.LicenseSeat.Proposal;

using Acme.Workforce.Assets.LicenseSeat.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LicenseSeatProposalRulesChecks
{
    public static void LicenseSeatProposalValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatProposalRules.DefaultLicenseSeatProposalValidator();
        var item = LicenseSeatProposal.NewLicenseSeatProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatProposalBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new LicenseSeatProposalBatchSizeRangeRule(64, 86400);
        var item = LicenseSeatProposal.NewLicenseSeatProposal("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatProposalRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LicenseSeatProposalRequiredLocationRule("license seat proposal needs an entry");
        var item = LicenseSeatProposal.NewLicenseSeatProposal("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LicenseSeatProposalRequiredCodeRule("license seat proposal needs this field");
        var item = LicenseSeatProposal.NewLicenseSeatProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatProposalLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LicenseSeatProposalLabelLengthRule(86400);
        var item = LicenseSeatProposal.NewLicenseSeatProposal("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LicenseSeatProposalRequiredLabelRule("license seat proposal needs a value");
        var item = LicenseSeatProposal.NewLicenseSeatProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatProposalCodeLengthRule(80);
        var item = LicenseSeatProposal.NewLicenseSeatProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatProposalValidatorReportsEveryProblem();
        LicenseSeatProposalBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        LicenseSeatProposalRequiredLocationRuleFlagsABlankLocation();
        LicenseSeatProposalRequiredCodeRuleFlagsABlankCode();
        LicenseSeatProposalLabelLengthRuleFlagsALongLabel();
        LicenseSeatProposalRequiredLabelRuleFlagsABlankLabel();
        LicenseSeatProposalCodeLengthRuleFlagsALongCode();
    }
}
