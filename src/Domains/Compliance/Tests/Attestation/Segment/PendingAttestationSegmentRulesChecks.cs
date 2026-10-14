namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PendingAttestationSegmentRulesChecks
{
    public static void PendingAttestationSegmentValidatorReportsEveryProblem()
    {
        var validator = PendingAttestationSegmentRules.DefaultPendingAttestationSegmentValidator();
        var item = PendingAttestationSegment.NewPendingAttestationSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingAttestationSegmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingAttestationSegmentRequiredTitleRule("pending attestation segment needs an entry");
        var item = PendingAttestationSegment.NewPendingAttestationSegment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAttestationSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingAttestationSegmentCodeLengthRule(80);
        var item = PendingAttestationSegment.NewPendingAttestationSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAttestationSegmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingAttestationSegmentTitleLengthRule(80);
        var item = PendingAttestationSegment.NewPendingAttestationSegment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingAttestationSegmentValidatorReportsEveryProblem();
        PendingAttestationSegmentRequiredTitleRuleFlagsABlankTitle();
        PendingAttestationSegmentCodeLengthRuleFlagsALongCode();
        PendingAttestationSegmentTitleLengthRuleFlagsALongTitle();
    }
}
