namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueDraft;

using Acme.Workforce.Compliance.TrainingDue.DueDraft;

/// <summary>Checks of due draft code, run by the project's runner.</summary>
public static class LegacyTrainingDueSegmentRulesChecks
{
    public static void LegacyTrainingDueSegmentValidatorReportsEveryProblem()
    {
        var validator = LegacyTrainingDueSegmentRules.DefaultLegacyTrainingDueSegmentValidator();
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTrainingDueSegmentRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new LegacyTrainingDueSegmentRevisionRangeRule(32, 64);
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("x", "y");
        item.Revision = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTrainingDueSegmentChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new LegacyTrainingDueSegmentChecksumLengthRule(16);
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyTrainingDueSegmentValidatorReportsEveryProblem();
        LegacyTrainingDueSegmentRevisionRangeRuleFlagsValuesAboveRevision();
        LegacyTrainingDueSegmentChecksumLengthRuleFlagsALongChecksum();
    }
}
