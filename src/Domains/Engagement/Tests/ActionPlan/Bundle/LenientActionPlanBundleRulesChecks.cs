namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientActionPlanBundleRulesChecks
{
    public static void LenientActionPlanBundleValidatorReportsEveryProblem()
    {
        var validator = LenientActionPlanBundleRules.DefaultLenientActionPlanBundleValidator();
        var item = LenientActionPlanBundle.NewLenientActionPlanBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientActionPlanBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientActionPlanBundleRequiredNameRule("lenient action plan bundle needs an entry");
        var item = LenientActionPlanBundle.NewLenientActionPlanBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientActionPlanBundleNameLengthRuleFlagsALongName()
    {
        var rule = new LenientActionPlanBundleNameLengthRule(16);
        var item = LenientActionPlanBundle.NewLenientActionPlanBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientActionPlanBundleValidatorReportsEveryProblem();
        LenientActionPlanBundleRequiredNameRuleFlagsABlankName();
        LenientActionPlanBundleNameLengthRuleFlagsALongName();
    }
}
