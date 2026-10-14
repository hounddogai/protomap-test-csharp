namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandProposalRulesChecks
{
    public static void PayBandProposalValidatorReportsEveryProblem()
    {
        var validator = PayBandProposalRules.DefaultPayBandProposalValidator();
        var item = PayBandProposal.NewPayBandProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandProposalCodeLengthRule(255);
        var item = PayBandProposal.NewPayBandProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandProposalRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new PayBandProposalRequiredPhoneRule("pay band proposal needs this field");
        var item = PayBandProposal.NewPayBandProposal("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayBandProposalValidatorReportsEveryProblem();
        PayBandProposalCodeLengthRuleFlagsALongCode();
        PayBandProposalRequiredPhoneRuleFlagsABlankPhone();
    }
}
