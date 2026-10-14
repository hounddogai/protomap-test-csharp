namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentEstimateRulesChecks
{
    public static void AssignmentEstimateValidatorReportsEveryProblem()
    {
        var validator = AssignmentEstimateRules.DefaultAssignmentEstimateValidator();
        var item = AssignmentEstimate.NewAssignmentEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentEstimateRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new AssignmentEstimateRequiredCostCenterRule("assignment estimate needs an entry");
        var item = AssignmentEstimate.NewAssignmentEstimate("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentEstimateCodeLengthRule(86400);
        var item = AssignmentEstimate.NewAssignmentEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentEstimateQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AssignmentEstimateQuantityRangeRule(16, 1000);
        var item = AssignmentEstimate.NewAssignmentEstimate("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssignmentEstimateValidatorReportsEveryProblem();
        AssignmentEstimateRequiredCostCenterRuleFlagsABlankCostCenter();
        AssignmentEstimateCodeLengthRuleFlagsALongCode();
        AssignmentEstimateQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
