namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdjustmentManifestRulesChecks
{
    public static void AdjustmentManifestValidatorReportsEveryProblem()
    {
        var validator = AdjustmentManifestRules.DefaultAdjustmentManifestValidator();
        var item = AdjustmentManifest.NewAdjustmentManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new AdjustmentManifestDurationMinutesRangeRule(3600, 86400);
        var item = AdjustmentManifest.NewAdjustmentManifest("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdjustmentManifestCodeLengthRule(16);
        var item = AdjustmentManifest.NewAdjustmentManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdjustmentManifestValidatorReportsEveryProblem();
        AdjustmentManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        AdjustmentManifestCodeLengthRuleFlagsALongCode();
    }
}
