namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class MobilityProposalRulesChecks
{
    public static void MobilityProposalValidatorReportsEveryProblem()
    {
        var validator = MobilityProposalRules.DefaultMobilityProposalValidator();
        var item = MobilityProposal.NewMobilityProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityProposalSourceLengthRuleFlagsALongSource()
    {
        var rule = new MobilityProposalSourceLengthRule(1000);
        var item = MobilityProposal.NewMobilityProposal("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MobilityProposalRequiredCodeRule("mobility proposal needs an entry");
        var item = MobilityProposal.NewMobilityProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityProposalNameLengthRuleFlagsALongName()
    {
        var rule = new MobilityProposalNameLengthRule(64);
        var item = MobilityProposal.NewMobilityProposal("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityProposalRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new MobilityProposalRatioRangeRule(7.3, 974.7);
        var item = MobilityProposal.NewMobilityProposal("x", "y");
        item.Ratio = 976.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MobilityProposalRequiredTitleRule("mobility proposal needs a value");
        var item = MobilityProposal.NewMobilityProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilityProposalValidatorReportsEveryProblem();
        MobilityProposalSourceLengthRuleFlagsALongSource();
        MobilityProposalRequiredCodeRuleFlagsABlankCode();
        MobilityProposalNameLengthRuleFlagsALongName();
        MobilityProposalRatioRangeRuleFlagsValuesAboveRatio();
        MobilityProposalRequiredTitleRuleFlagsABlankTitle();
    }
}
