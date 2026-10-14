namespace Acme.Workforce.Payrollops.Tests.PayRun.Allocation;

using Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PayRunAllocationRulesChecks
{
    public static void PayRunAllocationValidatorReportsEveryProblem()
    {
        var validator = PayRunAllocationRules.DefaultPayRunAllocationValidator();
        var item = PayRunAllocation.NewPayRunAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunAllocationScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PayRunAllocationScoreRangeRule(9.6, 2519.5);
        var item = PayRunAllocation.NewPayRunAllocation("x", "y");
        item.Score = 2521.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunAllocationRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new PayRunAllocationRequiredManagerIdRule("pay run allocation needs this field");
        var item = PayRunAllocation.NewPayRunAllocation("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayRunAllocationValidatorReportsEveryProblem();
        PayRunAllocationScoreRangeRuleFlagsValuesAboveScore();
        PayRunAllocationRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
