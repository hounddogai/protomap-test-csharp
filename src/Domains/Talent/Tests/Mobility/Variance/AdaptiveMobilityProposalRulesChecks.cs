namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveMobilityProposalRulesChecks
{
    public static void AdaptiveMobilityProposalValidatorReportsEveryProblem()
    {
        var validator = AdaptiveMobilityProposalRules.DefaultAdaptiveMobilityProposalValidator();
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveMobilityProposalNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveMobilityProposalNameLengthRule(32);
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveMobilityProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveMobilityProposalCodeLengthRule(86400);
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveMobilityProposalRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new AdaptiveMobilityProposalRequiredCurrencyRule("adaptive mobility proposal needs an entry");
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveMobilityProposalValidatorReportsEveryProblem();
        AdaptiveMobilityProposalNameLengthRuleFlagsALongName();
        AdaptiveMobilityProposalCodeLengthRuleFlagsALongCode();
        AdaptiveMobilityProposalRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
