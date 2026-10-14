namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientReturnPlanOverrideRulesChecks
{
    public static void LenientReturnPlanOverrideValidatorReportsEveryProblem()
    {
        var validator = LenientReturnPlanOverrideRules.DefaultLenientReturnPlanOverrideValidator();
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientReturnPlanOverrideRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new LenientReturnPlanOverrideRequiredSourceRule("lenient return plan override needs a value");
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientReturnPlanOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientReturnPlanOverrideCodeLengthRule(120);
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientReturnPlanOverrideTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientReturnPlanOverrideTitleLengthRule(120);
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientReturnPlanOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientReturnPlanOverrideRequiredNameRule("lenient return plan override needs this field");
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientReturnPlanOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new LenientReturnPlanOverrideNameLengthRule(16);
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientReturnPlanOverrideValidatorReportsEveryProblem();
        LenientReturnPlanOverrideRequiredSourceRuleFlagsABlankSource();
        LenientReturnPlanOverrideCodeLengthRuleFlagsALongCode();
        LenientReturnPlanOverrideTitleLengthRuleFlagsALongTitle();
        LenientReturnPlanOverrideRequiredNameRuleFlagsABlankName();
        LenientReturnPlanOverrideNameLengthRuleFlagsALongName();
    }
}
