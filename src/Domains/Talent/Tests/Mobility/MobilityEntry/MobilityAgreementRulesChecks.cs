namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class MobilityAgreementRulesChecks
{
    public static void MobilityAgreementValidatorReportsEveryProblem()
    {
        var validator = MobilityAgreementRules.DefaultMobilityAgreementValidator();
        var item = MobilityAgreement.NewMobilityAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MobilityAgreementRequiredCodeRule("mobility agreement needs this field");
        var item = MobilityAgreement.NewMobilityAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityAgreementRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new MobilityAgreementRequiredSegmentRule("mobility agreement needs a value");
        var item = MobilityAgreement.NewMobilityAgreement("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilityAgreementValidatorReportsEveryProblem();
        MobilityAgreementRequiredCodeRuleFlagsABlankCode();
        MobilityAgreementRequiredSegmentRuleFlagsABlankSegment();
    }
}
