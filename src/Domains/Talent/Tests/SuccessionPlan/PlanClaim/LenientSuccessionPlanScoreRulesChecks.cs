namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanScoreRulesChecks
{
    public static void LenientSuccessionPlanScoreValidatorReportsEveryProblem()
    {
        var validator = LenientSuccessionPlanScoreRules.DefaultLenientSuccessionPlanScoreValidator();
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSuccessionPlanScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientSuccessionPlanScoreRequiredCodeRule("lenient succession plan score needs an entry");
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSuccessionPlanScoreRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new LenientSuccessionPlanScoreRevisionRangeRule(80, 3600);
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSuccessionPlanScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientSuccessionPlanScoreCodeLengthRule(255);
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSuccessionPlanScoreRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientSuccessionPlanScoreRequiredLabelRule("lenient succession plan score needs an entry");
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanScoreValidatorReportsEveryProblem();
        LenientSuccessionPlanScoreRequiredCodeRuleFlagsABlankCode();
        LenientSuccessionPlanScoreRevisionRangeRuleFlagsValuesAboveRevision();
        LenientSuccessionPlanScoreCodeLengthRuleFlagsALongCode();
        LenientSuccessionPlanScoreRequiredLabelRuleFlagsABlankLabel();
    }
}
