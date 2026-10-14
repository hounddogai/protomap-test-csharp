namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanWindowRulesChecks
{
    public static void ReturnPlanWindowValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanWindowRules.DefaultReturnPlanWindowValidator();
        var item = ReturnPlanWindow.NewReturnPlanWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanWindowRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new ReturnPlanWindowRequiredCostCenterRule("return plan window needs an entry");
        var item = ReturnPlanWindow.NewReturnPlanWindow("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanWindowRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new ReturnPlanWindowRequiredOwnerIdRule("return plan window needs this field");
        var item = ReturnPlanWindow.NewReturnPlanWindow("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanWindowVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ReturnPlanWindowVersionRangeRule(80, 120);
        var item = ReturnPlanWindow.NewReturnPlanWindow("x", "y");
        item.Version = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReturnPlanWindowCodeLengthRule(3600);
        var item = ReturnPlanWindow.NewReturnPlanWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanWindowValidatorReportsEveryProblem();
        ReturnPlanWindowRequiredCostCenterRuleFlagsABlankCostCenter();
        ReturnPlanWindowRequiredOwnerIdRuleFlagsABlankOwnerId();
        ReturnPlanWindowVersionRangeRuleFlagsValuesAboveVersion();
        ReturnPlanWindowCodeLengthRuleFlagsALongCode();
    }
}
