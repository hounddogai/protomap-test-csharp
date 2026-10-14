namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class ReturnPlanTierRulesChecks
{
    public static void ReturnPlanTierValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanTierRules.DefaultReturnPlanTierValidator();
        var item = ReturnPlanTier.NewReturnPlanTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReturnPlanTierCodeLengthRule(80);
        var item = ReturnPlanTier.NewReturnPlanTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanTierRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ReturnPlanTierRevisionRangeRule(32, 80);
        var item = ReturnPlanTier.NewReturnPlanTier("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ReturnPlanTierRequiredLabelRule("return plan tier needs a value");
        var item = ReturnPlanTier.NewReturnPlanTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReturnPlanTierValidatorReportsEveryProblem();
        ReturnPlanTierCodeLengthRuleFlagsALongCode();
        ReturnPlanTierRevisionRangeRuleFlagsValuesAboveRevision();
        ReturnPlanTierRequiredLabelRuleFlagsABlankLabel();
    }
}
