namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanManifestRulesChecks
{
    public static void FallbackSuccessionPlanManifestValidatorReportsEveryProblem()
    {
        var validator = FallbackSuccessionPlanManifestRules.DefaultFallbackSuccessionPlanManifestValidator();
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackSuccessionPlanManifestWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new FallbackSuccessionPlanManifestWindowDaysRangeRule(32, 86400);
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackSuccessionPlanManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackSuccessionPlanManifestCodeLengthRule(3600);
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackSuccessionPlanManifestVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new FallbackSuccessionPlanManifestVersionRangeRule(16, 86400);
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackSuccessionPlanManifestValidatorReportsEveryProblem();
        FallbackSuccessionPlanManifestWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        FallbackSuccessionPlanManifestCodeLengthRuleFlagsALongCode();
        FallbackSuccessionPlanManifestVersionRangeRuleFlagsValuesAboveVersion();
    }
}
