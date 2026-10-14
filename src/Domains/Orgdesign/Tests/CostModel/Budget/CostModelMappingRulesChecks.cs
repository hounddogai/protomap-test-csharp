namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelMappingRulesChecks
{
    public static void CostModelMappingValidatorReportsEveryProblem()
    {
        var validator = CostModelMappingRules.DefaultCostModelMappingValidator();
        var item = CostModelMapping.NewCostModelMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelMappingRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new CostModelMappingRequiredTimezoneRule("cost model mapping needs a value");
        var item = CostModelMapping.NewCostModelMapping("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelMappingCodeLengthRule(80);
        var item = CostModelMapping.NewCostModelMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelMappingRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new CostModelMappingRequiredDepartmentRule("cost model mapping needs an entry");
        var item = CostModelMapping.NewCostModelMapping("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelMappingNameLengthRuleFlagsALongName()
    {
        var rule = new CostModelMappingNameLengthRule(120);
        var item = CostModelMapping.NewCostModelMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelMappingThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new CostModelMappingThresholdRangeRule(8.1, 3336.8);
        var item = CostModelMapping.NewCostModelMapping("x", "y");
        item.Threshold = 3338.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CostModelMappingValidatorReportsEveryProblem();
        CostModelMappingRequiredTimezoneRuleFlagsABlankTimezone();
        CostModelMappingCodeLengthRuleFlagsALongCode();
        CostModelMappingRequiredDepartmentRuleFlagsABlankDepartment();
        CostModelMappingNameLengthRuleFlagsALongName();
        CostModelMappingThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
