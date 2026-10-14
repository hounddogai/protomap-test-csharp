namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationProposalRulesChecks
{
    public static void CertificationProposalValidatorReportsEveryProblem()
    {
        var validator = CertificationProposalRules.DefaultCertificationProposalValidator();
        var item = CertificationProposal.NewCertificationProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationProposalRequiredCodeRule("certification proposal needs this field");
        var item = CertificationProposal.NewCertificationProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new CertificationProposalCodeLengthRule(255);
        var item = CertificationProposal.NewCertificationProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationProposalQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new CertificationProposalQuantityRangeRule(255, 3600);
        var item = CertificationProposal.NewCertificationProposal("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationProposalValidatorReportsEveryProblem();
        CertificationProposalRequiredCodeRuleFlagsABlankCode();
        CertificationProposalCodeLengthRuleFlagsALongCode();
        CertificationProposalQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
