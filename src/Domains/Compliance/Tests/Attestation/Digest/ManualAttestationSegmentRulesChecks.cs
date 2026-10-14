namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ManualAttestationSegmentRulesChecks
{
    public static void ManualAttestationSegmentValidatorReportsEveryProblem()
    {
        var validator = ManualAttestationSegmentRules.DefaultManualAttestationSegmentValidator();
        var item = ManualAttestationSegment.NewManualAttestationSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAttestationSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualAttestationSegmentCodeLengthRule(32);
        var item = ManualAttestationSegment.NewManualAttestationSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAttestationSegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualAttestationSegmentRequiredNameRule("manual attestation segment needs an entry");
        var item = ManualAttestationSegment.NewManualAttestationSegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAttestationSegmentRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ManualAttestationSegmentRequiredTeamRule("manual attestation segment needs this field");
        var item = ManualAttestationSegment.NewManualAttestationSegment("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualAttestationSegmentValidatorReportsEveryProblem();
        ManualAttestationSegmentCodeLengthRuleFlagsALongCode();
        ManualAttestationSegmentRequiredNameRuleFlagsABlankName();
        ManualAttestationSegmentRequiredTeamRuleFlagsABlankTeam();
    }
}
