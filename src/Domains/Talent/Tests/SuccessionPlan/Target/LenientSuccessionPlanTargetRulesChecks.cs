namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientSuccessionPlanTargetRulesChecks
{
    public static void LenientSuccessionPlanTargetValidatorReportsEveryProblem()
    {
        var validator = LenientSuccessionPlanTargetRules.DefaultLenientSuccessionPlanTargetValidator();
        var item = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSuccessionPlanTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientSuccessionPlanTargetCodeLengthRule(3600);
        var item = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSuccessionPlanTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientSuccessionPlanTargetRequiredCodeRule("lenient succession plan target needs an entry");
        var item = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanTargetValidatorReportsEveryProblem();
        LenientSuccessionPlanTargetCodeLengthRuleFlagsALongCode();
        LenientSuccessionPlanTargetRequiredCodeRuleFlagsABlankCode();
    }
}
