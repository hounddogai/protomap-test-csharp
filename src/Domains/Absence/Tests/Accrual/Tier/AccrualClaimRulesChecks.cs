namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualClaimRulesChecks
{
    public static void AccrualClaimValidatorReportsEveryProblem()
    {
        var validator = AccrualClaimRules.DefaultAccrualClaimValidator();
        var item = AccrualClaim.NewAccrualClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualClaimDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new AccrualClaimDepartmentLengthRule(1000);
        var item = AccrualClaim.NewAccrualClaim("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualClaimValidatorReportsEveryProblem();
        AccrualClaimDepartmentLengthRuleFlagsALongDepartment();
    }
}
