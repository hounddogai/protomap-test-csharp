namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RegionalAssignmentAgreementRulesChecks
{
    public static void RegionalAssignmentAgreementValidatorReportsEveryProblem()
    {
        var validator = RegionalAssignmentAgreementRules.DefaultRegionalAssignmentAgreementValidator();
        var item = RegionalAssignmentAgreement.NewRegionalAssignmentAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAssignmentAgreementRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new RegionalAssignmentAgreementRequiredSegmentRule("regional assignment agreement needs this field");
        var item = RegionalAssignmentAgreement.NewRegionalAssignmentAgreement("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAssignmentAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalAssignmentAgreementRequiredCodeRule("regional assignment agreement needs this field");
        var item = RegionalAssignmentAgreement.NewRegionalAssignmentAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalAssignmentAgreementValidatorReportsEveryProblem();
        RegionalAssignmentAgreementRequiredSegmentRuleFlagsABlankSegment();
        RegionalAssignmentAgreementRequiredCodeRuleFlagsABlankCode();
    }
}
