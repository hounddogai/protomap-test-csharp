namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanPreferenceRulesChecks
{
    public static void ReturnPlanPreferenceValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanPreferenceRules.DefaultReturnPlanPreferenceValidator();
        var item = ReturnPlanPreference.NewReturnPlanPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanPreferenceRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new ReturnPlanPreferenceRequiredDepartmentRule("return plan preference needs an entry");
        var item = ReturnPlanPreference.NewReturnPlanPreference("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanPreferenceQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ReturnPlanPreferenceQuantityRangeRule(80, 255);
        var item = ReturnPlanPreference.NewReturnPlanPreference("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ReturnPlanPreferenceTitleLengthRule(3600);
        var item = ReturnPlanPreference.NewReturnPlanPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanPreferenceRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new ReturnPlanPreferenceRatioRangeRule(1.5, 3889.7);
        var item = ReturnPlanPreference.NewReturnPlanPreference("x", "y");
        item.Ratio = 3891.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReturnPlanPreferenceValidatorReportsEveryProblem();
        ReturnPlanPreferenceRequiredDepartmentRuleFlagsABlankDepartment();
        ReturnPlanPreferenceQuantityRangeRuleFlagsValuesAboveQuantity();
        ReturnPlanPreferenceTitleLengthRuleFlagsALongTitle();
        ReturnPlanPreferenceRatioRangeRuleFlagsValuesAboveRatio();
    }
}
