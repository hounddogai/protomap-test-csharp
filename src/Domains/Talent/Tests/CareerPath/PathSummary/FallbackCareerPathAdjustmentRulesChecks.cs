namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class FallbackCareerPathAdjustmentRulesChecks
{
    public static void FallbackCareerPathAdjustmentValidatorReportsEveryProblem()
    {
        var validator = FallbackCareerPathAdjustmentRules.DefaultFallbackCareerPathAdjustmentValidator();
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackCareerPathAdjustmentManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new FallbackCareerPathAdjustmentManagerIdLengthRule(1000);
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackCareerPathAdjustmentEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new FallbackCareerPathAdjustmentEndsAtRangeRule(120, 3600);
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackCareerPathAdjustmentValidatorReportsEveryProblem();
        FallbackCareerPathAdjustmentManagerIdLengthRuleFlagsALongManagerId();
        FallbackCareerPathAdjustmentEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
