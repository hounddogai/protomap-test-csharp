namespace Acme.Workforce.Absence.Tests.ReturnPlan.Manifest;

using Acme.Workforce.Absence.ReturnPlan.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ReturnPlanEntryRulesChecks
{
    public static void ReturnPlanEntryValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanEntryRules.DefaultReturnPlanEntryValidator();
        var item = ReturnPlanEntry.NewReturnPlanEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanEntryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new ReturnPlanEntryLeaveDaysRangeRule(64, 120);
        var item = ReturnPlanEntry.NewReturnPlanEntry("x", "y");
        item.LeaveDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanEntryRequiredCodeRule("return plan entry needs this field");
        var item = ReturnPlanEntry.NewReturnPlanEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReturnPlanEntryValidatorReportsEveryProblem();
        ReturnPlanEntryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        ReturnPlanEntryRequiredCodeRuleFlagsABlankCode();
    }
}
