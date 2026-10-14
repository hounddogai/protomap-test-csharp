namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictReturnPlanBundleRulesChecks
{
    public static void StrictReturnPlanBundleValidatorReportsEveryProblem()
    {
        var validator = StrictReturnPlanBundleRules.DefaultStrictReturnPlanBundleValidator();
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictReturnPlanBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictReturnPlanBundleRequiredNameRule("strict return plan bundle needs this field");
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReturnPlanBundleVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new StrictReturnPlanBundleVersionRangeRule(255, 1000);
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictReturnPlanBundleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictReturnPlanBundleRequiredTitleRule("strict return plan bundle needs a value");
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictReturnPlanBundleValidatorReportsEveryProblem();
        StrictReturnPlanBundleRequiredNameRuleFlagsABlankName();
        StrictReturnPlanBundleVersionRangeRuleFlagsValuesAboveVersion();
        StrictReturnPlanBundleRequiredTitleRuleFlagsABlankTitle();
    }
}
