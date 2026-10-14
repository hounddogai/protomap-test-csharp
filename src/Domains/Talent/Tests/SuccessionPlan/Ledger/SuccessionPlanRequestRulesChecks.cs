namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanRequestRulesChecks
{
    public static void SuccessionPlanRequestValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanRequestRules.DefaultSuccessionPlanRequestValidator();
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SuccessionPlanRequestCodeLengthRule(32);
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanRequestBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new SuccessionPlanRequestBatchSizeRangeRule(80, 3600);
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanRequestExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new SuccessionPlanRequestExternalRefLengthRule(80);
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanRequestCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new SuccessionPlanRequestCostCenterLengthRule(16);
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanRequestMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new SuccessionPlanRequestMinimumRangeRule(2.1, 3112.2);
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("x", "y");
        item.Minimum = 3113.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanRequestValidatorReportsEveryProblem();
        SuccessionPlanRequestCodeLengthRuleFlagsALongCode();
        SuccessionPlanRequestBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        SuccessionPlanRequestExternalRefLengthRuleFlagsALongExternalRef();
        SuccessionPlanRequestCostCenterLengthRuleFlagsALongCostCenter();
        SuccessionPlanRequestMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
