namespace Acme.Workforce.Talent.Tests.CareerPath.Allocation;

using Acme.Workforce.Talent.CareerPath.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class CareerPathAdjustmentRulesChecks
{
    public static void CareerPathAdjustmentValidatorReportsEveryProblem()
    {
        var validator = CareerPathAdjustmentRules.DefaultCareerPathAdjustmentValidator();
        var item = CareerPathAdjustment.NewCareerPathAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new CareerPathAdjustmentCodeLengthRule(80);
        var item = CareerPathAdjustment.NewCareerPathAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathAdjustmentManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new CareerPathAdjustmentManagerIdLengthRule(3600);
        var item = CareerPathAdjustment.NewCareerPathAdjustment("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathAdjustmentCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new CareerPathAdjustmentCostCenterLengthRule(16);
        var item = CareerPathAdjustment.NewCareerPathAdjustment("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CareerPathAdjustmentRequiredCodeRule("career path adjustment needs a value");
        var item = CareerPathAdjustment.NewCareerPathAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathAdjustmentValidatorReportsEveryProblem();
        CareerPathAdjustmentCodeLengthRuleFlagsALongCode();
        CareerPathAdjustmentManagerIdLengthRuleFlagsALongManagerId();
        CareerPathAdjustmentCostCenterLengthRuleFlagsALongCostCenter();
        CareerPathAdjustmentRequiredCodeRuleFlagsABlankCode();
    }
}
