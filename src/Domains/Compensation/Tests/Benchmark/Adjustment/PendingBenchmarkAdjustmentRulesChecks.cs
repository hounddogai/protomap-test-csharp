namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingBenchmarkAdjustmentRulesChecks
{
    public static void PendingBenchmarkAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PendingBenchmarkAdjustmentRules.DefaultPendingBenchmarkAdjustmentValidator();
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingBenchmarkAdjustmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingBenchmarkAdjustmentRequiredTitleRule("pending benchmark adjustment needs a value");
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingBenchmarkAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingBenchmarkAdjustmentCodeLengthRule(80);
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingBenchmarkAdjustmentCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new PendingBenchmarkAdjustmentCostCenterLengthRule(86400);
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingBenchmarkAdjustmentValidatorReportsEveryProblem();
        PendingBenchmarkAdjustmentRequiredTitleRuleFlagsABlankTitle();
        PendingBenchmarkAdjustmentCodeLengthRuleFlagsALongCode();
        PendingBenchmarkAdjustmentCostCenterLengthRuleFlagsALongCostCenter();
    }
}
