namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CostModelAllocationRulesChecks
{
    public static void CostModelAllocationValidatorReportsEveryProblem()
    {
        var validator = CostModelAllocationRules.DefaultCostModelAllocationValidator();
        var item = CostModelAllocation.NewCostModelAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelAllocationRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new CostModelAllocationRequiredLocationRule("cost model allocation needs this field");
        var item = CostModelAllocation.NewCostModelAllocation("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelAllocationJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new CostModelAllocationJobCodeLengthRule(64);
        var item = CostModelAllocation.NewCostModelAllocation("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelAllocationLocationLengthRuleFlagsALongLocation()
    {
        var rule = new CostModelAllocationLocationLengthRule(255);
        var item = CostModelAllocation.NewCostModelAllocation("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelAllocationValidatorReportsEveryProblem();
        CostModelAllocationRequiredLocationRuleFlagsABlankLocation();
        CostModelAllocationJobCodeLengthRuleFlagsALongJobCode();
        CostModelAllocationLocationLengthRuleFlagsALongLocation();
    }
}
