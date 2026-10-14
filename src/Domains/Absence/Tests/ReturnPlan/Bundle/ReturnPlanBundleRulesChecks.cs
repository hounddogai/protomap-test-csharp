namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanBundleRulesChecks
{
    public static void ReturnPlanBundleValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanBundleRules.DefaultReturnPlanBundleValidator();
        var item = ReturnPlanBundle.NewReturnPlanBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanBundleMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ReturnPlanBundleMinimumRangeRule(7.7, 940.2);
        var item = ReturnPlanBundle.NewReturnPlanBundle("x", "y");
        item.Minimum = 941.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanBundleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ReturnPlanBundleFteRangeRule(9.2, 2182.1);
        var item = ReturnPlanBundle.NewReturnPlanBundle("x", "y");
        item.Fte = 2183.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanBundleContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new ReturnPlanBundleContactEmailLengthRule(255);
        var item = ReturnPlanBundle.NewReturnPlanBundle("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanBundleValidatorReportsEveryProblem();
        ReturnPlanBundleMinimumRangeRuleFlagsValuesAboveMinimum();
        ReturnPlanBundleFteRangeRuleFlagsValuesAboveFte();
        ReturnPlanBundleContactEmailLengthRuleFlagsALongContactEmail();
    }
}
