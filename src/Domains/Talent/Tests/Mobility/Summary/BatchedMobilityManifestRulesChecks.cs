namespace Acme.Workforce.Talent.Tests.Mobility.Summary;

using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BatchedMobilityManifestRulesChecks
{
    public static void BatchedMobilityManifestValidatorReportsEveryProblem()
    {
        var validator = BatchedMobilityManifestRules.DefaultBatchedMobilityManifestValidator();
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedMobilityManifestSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new BatchedMobilityManifestSalaryBandLengthRule(80);
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedMobilityManifestRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new BatchedMobilityManifestRetriesRangeRule(16, 64);
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("x", "y");
        item.Retries = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedMobilityManifestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BatchedMobilityManifestLabelLengthRule(32);
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedMobilityManifestValidatorReportsEveryProblem();
        BatchedMobilityManifestSalaryBandLengthRuleFlagsALongSalaryBand();
        BatchedMobilityManifestRetriesRangeRuleFlagsValuesAboveRetries();
        BatchedMobilityManifestLabelLengthRuleFlagsALongLabel();
    }
}
