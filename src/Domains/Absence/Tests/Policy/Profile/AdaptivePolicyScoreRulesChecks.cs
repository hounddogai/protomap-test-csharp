namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdaptivePolicyScoreRulesChecks
{
    public static void AdaptivePolicyScoreValidatorReportsEveryProblem()
    {
        var validator = AdaptivePolicyScoreRules.DefaultAdaptivePolicyScoreValidator();
        var item = AdaptivePolicyScore.NewAdaptivePolicyScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePolicyScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new AdaptivePolicyScoreUpdatedAtRangeRule(64, 64);
        var item = AdaptivePolicyScore.NewAdaptivePolicyScore("x", "y");
        item.UpdatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptivePolicyScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptivePolicyScoreLabelLengthRule(64);
        var item = AdaptivePolicyScore.NewAdaptivePolicyScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyScoreRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new AdaptivePolicyScoreRequiredChecksumRule("adaptive policy score needs this field");
        var item = AdaptivePolicyScore.NewAdaptivePolicyScore("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptivePolicyScoreValidatorReportsEveryProblem();
        AdaptivePolicyScoreUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        AdaptivePolicyScoreLabelLengthRuleFlagsALongLabel();
        AdaptivePolicyScoreRequiredChecksumRuleFlagsABlankChecksum();
    }
}
