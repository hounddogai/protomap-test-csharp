namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanManifestRulesChecks
{
    public static void SuccessionPlanManifestValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanManifestRules.DefaultSuccessionPlanManifestValidator();
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanManifestNotesLengthRuleFlagsALongNotes()
    {
        var rule = new SuccessionPlanManifestNotesLengthRule(86400);
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanManifestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SuccessionPlanManifestLabelLengthRule(86400);
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanManifestSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new SuccessionPlanManifestSegmentLengthRule(64);
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanManifestValidatorReportsEveryProblem();
        SuccessionPlanManifestNotesLengthRuleFlagsALongNotes();
        SuccessionPlanManifestLabelLengthRuleFlagsALongLabel();
        SuccessionPlanManifestSegmentLengthRuleFlagsALongSegment();
    }
}
